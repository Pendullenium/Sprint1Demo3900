using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class ChestPassBall : MonoBehaviour
{
    private ChestPassGame game;
    private bool resolved;

    public void Initialise(ChestPassGame owner)
    {
        game = owner;
        resolved = false;
    }

    public void Catch()
    {
        if (resolved || game == null) return;
        resolved = true;
        game.ResolveBall(this, true);
    }

    public void Miss()
    {
        if (resolved || game == null) return;
        resolved = true;
        game.ResolveBall(this, false);
    }
}
