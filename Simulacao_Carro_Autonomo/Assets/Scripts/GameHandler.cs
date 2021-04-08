using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe responsavel pela logica da simulação
public class GameHandler : MonoBehaviour
{

    

   //Variavel responsavel para inicar o Client
    private bool StartClient = false;
    //Variavel responsavel por mostrar o debug
    private bool debuglog = false;

    //Obejeto da classe velocimetro
    private Velocimetro vel;

    private void Awake()
    {
        //Acha o objeto em cena com o script Velocimetro responsavel pela UI
        vel = GameObject.FindObjectOfType<Velocimetro>().GetComponent<Velocimetro>();

    }
    void Start()
    {
        //se inscreve ao evento onTick da classe TimerSystem
        TimerSystem.onTick += TimerSystem_onTick;
    }

    //Evento onTick 
    private void TimerSystem_onTick(object sender, System.EventArgs e)
    {
        //Vê se podesse conectar com o server
        if (StartClient)
        {
            //Função feita para conectar e mandar uma imagem 500x300 para o python 
            GetImag.TakeImage_S(500, 300);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vê se a tecla F3 foi precionada se sim, troca o estado da variavel StartClient
        if (Input.GetKeyDown(KeyCode.F3))
        {
            StartClient = !StartClient;
        }

        //Vê se a tecla D foi precionada se sim, troca o estado da variavel debuglog, e ativa ou dessativa a UI
        if (Input.GetKeyDown(KeyCode.D))
        {
            debuglog = !debuglog;
            if (!debuglog)
            {
                vel.Hide();
            }

            else
            {
                vel.Show();

            }
        }

    }
}
