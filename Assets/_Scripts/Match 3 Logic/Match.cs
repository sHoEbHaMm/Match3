using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum Orientation
{
    none, 
    horizontal, 
    vertical, 
    both
}

/*
 * This is a collection of Matchables that have been matched
 */
public class Match
{
    // the number of matchables that are considered part of this match but aren't added to the list
    private int unlisted = 0;

    // is this match horizontal or vertical?
    public Orientation orientation = Orientation.none;

    // the internal list of matched matchables
    private List<Matchable> matchables;

    // getters for the list and the list count
    public List<Matchable> Matchables
    {
        get
        {
            return matchables;
        }
    }

    //  getter for number of matchables part of this match
    public int Count
    {
        get
        {
            return matchables.Count + unlisted;
        }
    }

    // constructor, initializes the lis
    public Match()
    {
        matchables = new List<Matchable>(5);
    }

    // overload, also adds a matchable
    public Match(Matchable original) : this()
    {
        AddMatchable(original);
    }

    // add a matchable to the list
    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }

    // add a matchable to the count without adding it to the list
    public void AddUnlisted()
    {
        unlisted++;
    }

    // merge another match into this one
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

    //  check if a matchable is already in this match
    public bool Contains(Matchable toCompare)
    {
        return matchables.Contains(toCompare);
    }

}
