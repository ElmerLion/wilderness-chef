using UnityEngine;

public class UnlockableArea : MonoBehaviour {

    public RankManager.Rank rank { get; private set; }

    public void Initialize(RankManager.Rank rank) {
        this.rank = rank;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Player player)) {
            int neededMeals = rank.minMealsDelivered - RankManager.Instance.GetDeliveredMeals();
            MessageUI.Instance.ShowMessage($"You need to be a {rank.name} to enter this area! Deliver {neededMeals} more meals to gain access.", 4f);
        }
    }

}
