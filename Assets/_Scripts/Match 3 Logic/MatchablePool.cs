using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private MatchableType[] matchableTypes;

    public void ChooseARandomType(Matchable matchableToRandomize)
    {
        int random = UnityEngine.Random.Range(0, matchableTypes.Length);

        matchableToRandomize.SetType(matchableTypes[random]);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();
        ChooseARandomType(randomMatchable);
        return randomMatchable;
    }

    public MatchableType NextType(Matchable matchable)
    {
        int i;
        for(i = 0; i < matchableTypes.Length; i++)
        {
            if (matchableTypes[i] == matchable.GetMatchableType())
            {
                break;
            }
        }

        MatchableType nextType = matchableTypes[(i + 1) % matchableTypes.Length];

        matchable.SetType(nextType);

        return nextType;

    }
}
