
public class CollectableObject : InteractableObject
{
    public EnumerableItem[] Items;

    public override string Action => "Collect";

    public override float InteractionTime => 1;

    public override void OnInteractingFinished()
    {
        base.OnInteractingFinished();
        transform.parent.gameObject.SetActive(false);
    }
}
