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
    if (game == null || !game.IsContractSigned) return;

    if (Input.GetKeyDown(KeyCode.H)) game.PlayerHit();
    if (Input.GetKeyDown(KeyCode.S)) game.PlayerStand();
    if (Input.GetKeyDown(KeyCode.N)) game.StartRound(); // N = next round
}

public void Hit()
{
    if (game == null || !game.IsContractSigned) return;
    game.PlayerHit();
}

public void Stand()
{
    if (game == null || !game.IsContractSigned) return;
    game.PlayerStand();
}
}