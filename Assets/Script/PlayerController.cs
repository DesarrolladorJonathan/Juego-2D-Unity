using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direccion;
    private Vector2 direccionRaw;
    private Animator animacion;
    private CinemachineVirtualCamera cm;
    private Vector2 direccionMovimiento;


    [Header("Estadisticas")]
    public float velocidadDeMovimiento = 10;
    public float fuerzaDeSalto = 5;
    public float velocidadRodar = 20;

    [Header("Booleanos")]
    public bool puedeMover = true;
    public bool enSuelo = true;
    public bool puedeRodar;
    public bool hacerRodar;
    public bool tocadoPido;
    public bool haciendoShake;
    public bool isAtacando;


    [Header("Colisiones")]
    public Vector2 abajo;
    public float radioColision;
    public LayerMask layerPiso;
   

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animacion = GetComponent<Animator>();
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Movimiento();
        Agarres();
    }

    private void Movimiento() {

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        float xRow = Input.GetAxisRaw("Horizontal");
        float yRow = Input.GetAxisRaw("Vertical");

        direccion = new Vector2(x, y);
        direccionRaw = new Vector2(xRow, yRow);

        Caminar();

        Atacar(DireccionAtaque(direccionMovimiento, direccionRaw));

        MejorarSalto();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (enSuelo)
            {
                animacion.SetBool("Saltar", true);
                Saltar();
            }
        }


        if (Input.GetKeyDown(KeyCode.X) && !hacerRodar)
        {
            if (xRow != 0 || yRow != 0)
            {
                Dash(xRow, yRow);
            }
        }

        if (enSuelo && !tocadoPido)
        {
            TocaPiso();
            tocadoPido = true;
        }

        if (!enSuelo && tocadoPido)
        {
            tocadoPido = false;
        }

        float velocidad;

        if (rb.velocity.y > 0)
        {
            velocidad = 1;
        }
        else
        {
            velocidad = -1;
        }
        if (!enSuelo)
        {

            animacion.SetFloat("VelocidadVertical", velocidad);
        }
        else
        {
            if (velocidad == -1)
            {
                FinalizarSalto();
            }

        }

    }

    public void FinalizarSalto()
    {
        animacion.SetBool("Saltar", false);
    }
    private void Caminar() {

        if (puedeMover && !hacerRodar)
        {
            rb.velocity = new Vector2(direccion.x * velocidadDeMovimiento, rb.velocity.y);

            if (direccion != Vector2.zero)
            {
                if (!enSuelo)
                {
                    animacion.SetBool("Saltar", true);
                }
                else {
                    animacion.SetBool("Caminar", true);
                }

                if (direccion.x < 0 && transform.localScale.x > 0)
                {
                    direccionMovimiento = DireccionAtaque(Vector2.left, direccion);
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
                else if (direccion.x > 0 && transform.localScale.x < 0) {
                    direccionMovimiento = DireccionAtaque(Vector2.right, direccion);
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                if (direccion.y > 0 && direccion.x ==0)
                {
                    direccionMovimiento = DireccionAtaque(direccion, Vector2.up);
                }
                animacion.SetBool("Caminar", false);
            }
        }


    }
    private void Saltar() {

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += Vector2.up * fuerzaDeSalto;

    }
    private void MejorarSalto()
    {
        if (rb.velocity.y < 0)
        {
            // significa que esta cayendo
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space)) {
            // significa que esta Saltando
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.0f - 1) * Time.deltaTime;
        }
    }
    private void Agarres() {

        enSuelo = Physics2D.OverlapCircle((Vector2)transform.position + abajo, radioColision, layerPiso);
    }

    private void Dash(float x, float y) {

        animacion.SetBool("Rodar", true);
        Vector3 posicionJugador = Camera.main.WorldToViewportPoint(transform.position);

        Camera.main.GetComponent<RippleEffect>().Emit(posicionJugador);
        StartCoroutine(AgitarCamara());

        puedeRodar = true;
        rb.velocity = Vector2.zero;
        rb.velocity += new Vector2(x, y).normalized * velocidadRodar;
        StartCoroutine(PrepararRodar());
    }
    private IEnumerator PrepararRodar()
    {
        StartCoroutine(RodarSuelo());

        rb.gravityScale = 0;
        hacerRodar = true;
        yield return new WaitForSeconds(0.35f);
        rb.gravityScale = 1;
        hacerRodar = false;
        FinalizarRodar();
    }
    private IEnumerator RodarSuelo() {
        yield return new WaitForSeconds(0.15f);

        if (enSuelo)
        {
            puedeRodar = false;
        }
    }

    private void TocaPiso()
    {
        puedeRodar = false;
        hacerRodar = false;
        animacion.SetBool("Saltar", false);
    }

    public void FinalizarRodar() {

        animacion.SetBool("Rodar", false);
    }

    private IEnumerator AgitarCamara()
    {
        haciendoShake = true;
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;
        yield return new WaitForSeconds(0.3f);
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        haciendoShake = false;
    }

    private IEnumerator AgitarCamara(float tiempo)
    {
        haciendoShake = true;
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;
        yield return new WaitForSeconds(tiempo);
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        haciendoShake = false;
    }


    private void Atacar(Vector2 direccion) {

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!isAtacando && !hacerRodar)
            {
                isAtacando = true;
                animacion.SetFloat("AtaqueX", direccion.x);
                animacion.SetFloat("AtaqueY", direccion.y);

                animacion.SetBool("Atacar", true);
            }
        }

    }

    public void FinalizarAtaque()
    {

        animacion.SetBool("Atacar", false);
        isAtacando = false;

    }


    private Vector2 DireccionAtaque(Vector2 direccionMovimiento, Vector2 direccionRaw)
    {

        if (rb.velocity.x ==0 && rb.velocity.y != 0)
        {
            return new Vector2(0, direccion.y);
        }

        return new Vector2(direccionMovimiento.x, direccion.y);
  
    }
}
