using UnityEngine;

public class InputTester : MonoBehaviour
{
    private BlackjackGame game;

    private void Start()
    {
        game = GetComponent<BlackjackGame>();
    }

    private void Update()
    {
        if (game == null || !game.IsContractSigned) return;

        if (Input.GetKeyDown(KeyCode.H)) game.PlayerHit();
        if (Input.GetKeyDown(KeyCode.S)) game.PlayerStand();
        if (Input.GetKeyDown(KeyCode.N)) game.StartRound();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Debug Testing Hotkeys
        if (Input.GetKeyDown(KeyCode.W)) game.DebugForceWin();
        if (Input.GetKeyDown(KeyCode.L)) game.DebugForceLoss();
        if (Input.GetKeyDown(KeyCode.V)) game.DebugInstantVictory();
        if (Input.GetKeyDown(KeyCode.K)) game.DebugInstantDefeat();
#endif
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