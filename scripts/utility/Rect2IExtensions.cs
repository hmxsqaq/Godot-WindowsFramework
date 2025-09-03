using System.Collections.Generic;
using Godot;

namespace windows_framework.scripts.utility;

public static class Rect2IExtensions
{
    public static List<Rect2I> Subtract(this Rect2I original, List<Rect2I> subtracts)
    {
        var resultRects = new List<Rect2I> { original };
        foreach (var subtractRect in subtracts)
        {
            var nextResultRects = new List<Rect2I>();
            foreach (var currentRect in resultRects) 
                nextResultRects.AddRange(currentRect.Subtract(subtractRect));
            resultRects = nextResultRects;
        }
        return resultRects;
    }
    
    public static List<Rect2I> Subtract(this Rect2I original, Rect2I subtract)
    {
        List<Rect2I> result = [];
        if (!original.Intersects(subtract))
        {
            result.Add(original);
            return result;
        }
        var intersection = original.Intersection(subtract);
        // top
        if (original.Position.Y < intersection.Position.Y)
        {
            Rect2I topRect = new(
                original.Position,
                new Vector2I(original.Size.X, intersection.Position.Y - original.Position.Y)
            );
            result.Add(topRect);
        }
        // bottom
        if (original.End.Y > intersection.End.Y)
        {
            Rect2I bottomRect = new(
                new Vector2I(original.Position.X, intersection.End.Y),
                new Vector2I(original.Size.X, original.End.Y - intersection.End.Y)
            );
            result.Add(bottomRect);
        }
        // left
        if (original.Position.X < intersection.Position.X)
        {
            Rect2I leftRect = new(
                new Vector2I(original.Position.X, intersection.Position.Y),
                new Vector2I(intersection.Position.X - original.Position.X, intersection.Size.Y)
            );
            result.Add(leftRect);
        }
        // right
        if (original.End.X > intersection.End.X)
        {
            Rect2I rightRect = new(
                new Vector2I(intersection.End.X, intersection.Position.Y),
                new Vector2I(original.End.X - intersection.End.X, intersection.Size.Y)
            );
            result.Add(rightRect);
        }
        return result;
    }

}