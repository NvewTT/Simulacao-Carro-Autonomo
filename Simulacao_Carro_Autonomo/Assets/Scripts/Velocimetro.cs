using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Velocimetro : MonoBehaviour
{
    // Start is called before the first frame update

    
    private TextMeshProUGUI text;
    

    public static Velocimetro instancia;
    private Text textodebug;
    private void Awake()
    {
        instancia = this;

        text = GetComponentInChildren<TextMeshProUGUI>();
        textodebug = GetComponentInChildren<Text>();

    }
    void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Settext(List<Vector3> pose)
    {
        textodebug.text = "Posição: " +  pose[0].ToString() + "\nOrientação: " + pose[1].ToString() + "\nVelocidade: " + pose[2].ToString() + "\nVelocidade Angular: " + pose[3].ToString() +"\n";  

    }


    private void Update()
    {
        text.text = Carro.getVelocitS().ToString().Substring(0,5) + "Us";
        Settext(Carro.GetAllposS());
    }
    // Update is called once per frame

}
