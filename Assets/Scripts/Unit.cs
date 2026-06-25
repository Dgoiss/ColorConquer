// Atributos de unidade.
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Identidade")]
    public string NomeUnidade;
    public Color CorFaccao;

    [Header("Atributos de Ataque")]
    public int PoderAtaque;
    public float distanciaAtaque;
    public int acoesPorTurno;

    [Header("Atributos de Defesa")]
    public int PontosVida;
    public int vidaMaxia;
    public int valorDefesa;

    [Header("Mecânica Anti-Imortalidade")]
    public float nivelRecurso = 1.0f;
    public bool estaCercado;
}