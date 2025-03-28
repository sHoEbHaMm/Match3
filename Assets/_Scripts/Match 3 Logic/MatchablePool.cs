using UnityEngine;

/*
 * This is a pool of matchables which will be instantiated during load time.
 * Remember to always activate each game object before requesting a new one.
 * This class is also a Singleton which can be accessed through Instance.
 * 
 * This class also handles the types, sprites, and colours of the matchables.
 * The type can be randomized or incremented.
 */

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private MatchableType[] matchableTypes;

    public MatchableType[] MatchableTypes
    {
        get
        {
            return matchableTypes;
        }
    }

    //  Randomize the type of a matchable
    public void ChooseARandomType(Matchable matchableToRandomize)
    {
        int random = UnityEngine.Random.Range(0, matchableTypes.Length);

        matchableToRandomize.SetType(matchableTypes[random]);
    }

    //  Get a matchable from the pool and randomize its type
    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();
        ChooseARandomType(randomMatchable);
        return randomMatchable;
    }

    //  Increment the type of a matchable and return its new type
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

    //  Manually set the type of a matchable, used for testing obscure cases
    public void ChangeType(Matchable toChange, MatchableType type)
    {
        toChange.SetType(type);
    }
}
