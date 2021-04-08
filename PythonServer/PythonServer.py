import socket
import json
import numpy as np
import base64
import time
import math
from datetime import datetime
from PIL import Image
import cv2
from io import BytesIO
from threading import Thread

HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 4545         # The port used by the server
# variavel global responsavel pelo erro do sistema
ERROR = 0
# Cria uma matriz vazia de 2x2 e uma variavel global será usada para armazenar a imagem
# mandada pela unity
cvim = np.empty([2, 2])


# -------------------------------------------------------------------------------------
# Converte as inclinações e interseções das linha para coordenadas cartezianas
def make_coord(image, line_paraments):
    slope, intercept = line_paraments
    y1 = image.shape[0]
    y2 = int(y1 * (3 / 5))
    x1 = int((y1 - intercept) / slope)
    x2 = int((y2 - intercept) / slope)
    if (x1 < 0):
        n = y1 - (slope * x1)
        y1 = int(n)
        x1 = 0
    if (x1 > 500):
        n = y1 - (slope * x1)
        x1 = 500
        y1 = int((slope * x1) + n)

    # print(slope, [x1, y1, x2, y2])
    return np.array([x1, y1, x2, y2])
# -------------------------------------------------------------------------------------

# -------------------------------------------------------------------------------------
# Função responsavel por fazer a media das inclinações e interseções das linha


def average_slope_intercept(image, lines):
    left_fit = []
    right_fit = []

    for line in lines:
        x1, y1, x2, y2 = line.reshape(4)
        parameters = np.polyfit((x1, x2), (y1, y2), 1)
        slope = parameters[0]
        intercept = parameters[1]
        if slope < 0:
            left_fit.append((slope, intercept))
        else:
            right_fit.append((slope, intercept))
    left_fit_average = np.average(left_fit, axis=0)
    right_fit_average = np.average(right_fit, axis=0)
    left_line = make_coord(image, left_fit_average)

    right_line = make_coord(image, right_fit_average)
    return np.array([left_line, right_line])
# -------------------------------------------------------------------------------------

# -------------------------------------------------------------------------------------
# Função responsavel por desenhar as linhas na imagem


def display_line(image, lines):
    line_image = np.zeros_like(image)
    if lines is not None:
        for line in lines:
            x1, y1, x2, y2 = line.reshape(4)
            cv2.line(line_image, (x1, y1), (x2, y2), (255, 0, 0), 10)
    return line_image
# -------------------------------------------------------------------------------------

# -------------------------------------------------------------------------------------
# Função responsavel por recortar a imagem na area de interese


def region_of_interest(img, vertices):
    # Define a blank matrix that matches the image height/width.
    mask = np.zeros_like(img)
    # Retrieve the number of color channels of the image.
    channel_count = 1
    # Create a match color with the same color channel counts.
    match_mask_color = (255,) * channel_count

    # Fill inside the polygon
    cv2.fillPoly(mask, vertices, match_mask_color)

    # Returning the image only where mask pixels match
    masked_image = cv2.bitwise_and(img, mask)
    return masked_image
# -------------------------------------------------------------------------------------

# -------------------------------------------------------------------------------------
# Funçao que lê uma imagem em base 64 e converte para uma imagem


def readb64(base64_string):
    sbuf = BytesIO()
    sbuf.write(base64.b64decode(base64_string))
    pimg = Image.open(sbuf)
    return cv2.cvtColor(np.array(pimg), cv2.COLOR_RGB2BGR)
# -------------------------------------------------------------------------------------

# -------------------------------------------------------------------------------------
# Server Socket


def server_socket():
    global cvim
    global ERROR
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((HOST, PORT))
        s.listen()
        while 1:  # Accept connections from multiple clients
            # print('Listening for client...')
            conn, addr = s.accept()
            # print('Connection address:', addr)
            # Accept multiple messages from each client
            buffer = conn.recv(100000000)
            buffer1 = buffer.decode('utf-8')
            # imagem vinda da unity em base 64
            cvim = readb64(buffer1)
            # Converte o ERROR que está em float para bytes array
            error = bytes(str(ERROR), 'utf-8')
            # Envia o error para a unity
            conn.send(error)
            # fecha a conexão
            conn.close()


# -------------------------------------------------------------------------------------
# -------------------------------------------------------------------------------------
# Tread responsavel por executar o server
t1 = Thread(target=server_socket)
t1.start()
# -------------------------------------------------------------------------------------

while 1:
    # Vê se a imagem foi atualizada pela unity
    if len(cvim) > 2:
        # -----------------------------------------------------------------------------------------
        # O try é usado  para evitar travamentos quando a função de geração de linhas não encontrar
        # nenhuma linha valida na imagem.
        try:
            # -----------------------------------------------------------------------------------------
            # copia a imagem vinda da unity para uma nova imagem
            lane_image = np.copy(cvim)
            # Converte a imagem para HSV
            hsv = cv2.cvtColor(cvim, cv2.COLOR_BGR2HSV)
            # -------------------------------------------------------------------------------------
            # sensibilidade as marcações da rua
            sensitivity = 15  # melhor valor achado experimentalmentalmente para a pista 1
            # sensitivity = 15  # melhor valor achado experimentalmentalmente para a pista 2
            # -------------------------------------------------------------------------------------
            # valor mais baixo de branco a ser filtrado
            lower_white = np.array([0, 0, 255-sensitivity])
            # Valor mais alto de branco a ser filtrado
            upper_white = np.array([255, sensitivity, 255])
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # imagem filtrada utilizando os valores mais baixos e altos para branco
            mask = cv2.inRange(hsv, lower_white, upper_white)
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Região de interesse na imagem, cada tupla é uma vertex de um quadrado
            region_of_interest_vertices = [
                (0, 300),
                (0, (300 + 35) / 2),
                (500, (300 + 35) / 2),
                (500, 300),
            ]
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Recortando a imagem na zona de interesse
            cropped_image = region_of_interest(
                mask,
                np.array([region_of_interest_vertices], np.int32),
            )
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Função que descobre as linhas na imagem
            lines = cv2.HoughLinesP(
                cropped_image, 2, np.pi / 180, 100, np.array([]), 50, 10)
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Descobre a media das inclinações e interseções das linhas
            average_lines = average_slope_intercept(lane_image, lines)
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Erro entre o meio das linhas e da imagem (250) em porcentagem
            ERROR = (
                ((average_lines[0][2] + average_lines[1][2]) / 2) - 250) / 100
            # -------------------------------------------------------------------------------------

            # -------------------------------------------------------------------------------------
            # Funções responsaveis para mostrar as linhas geradas e adicionar em uma imagem
            line_image = display_line(lane_image, average_lines)
            combo_image = cv2.addWeighted(lane_image, 0.8, line_image, 1, 1)
            # -------------------------------------------------------------------------------------

        except:
            # -------------------------------------------------------------------------------------
            # Erro entre o meio das linhas e da imagem (250) em porcentagem
            ERROR = (
                ((average_lines[0][2] + average_lines[1][2]) / 2) - 250) / 100
            # -------------------------------------------------------------------------------------
        # -------------------------------------------------------------------------------------
        # Mostra imagem resultante com as linhas
        cv2.imshow("Result Image", combo_image)
        # -------------------------------------------------------------------------------------
        # -------------------------------------------------------------------------------------
        k = cv2.waitKey(1) & 0XFF
        if k == 27:
            break
cv2.destroyAllWindows()
t1.join(0.1)
