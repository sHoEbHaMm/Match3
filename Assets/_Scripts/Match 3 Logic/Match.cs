using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    private List<Matchable> matchables;

    public List<Matchable> Matchables
    {
        get
        {
            return matchables;
        }
    }

    public int Count
    {
        get
        {
            return matchables.Count;
        }
    }

    public Match()
    {
        matchables = new List<Matchable>(5);
    }

    public Match(Matchable original) : this()
    {
        AddMatchable(original);
    }

    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }

    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.matchables);
    }

    // convert the match into a string so we can see it
    public override string ToString()
    {
        string s = "Match of type " + matchables[0].GetMatchableType() + " : ";

        foreach (Matchable m in matchables)
            s += "(" + m.gridPosition.x + ", " + m.gridPosition.y + ") ";

        return s;
    }

}
