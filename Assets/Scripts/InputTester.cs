using UnityEngine;

public class InputTester : MonoBehaviour
{
    private BlackjackGame game;

    void Start()
    {
        game = GetComponent<BlackjackGame>();
    }

void Update()
{
    if (Input.GetKeyDown(KeyCode.H)) game.PlayerHit();
    if (Input.GetKeyDown(KeyCode.S)) game.PlayerStand();
    if (Input.GetKeyDown(KeyCode.N)) game.StartRound(); // N = next round
}
}