public interface IItem {
    bool IsPickedUp();
    void PickUp();
    string GetInteractMessage();
}