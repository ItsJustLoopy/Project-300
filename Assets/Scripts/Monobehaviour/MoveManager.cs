using System.Collections.Generic;
using UnityEngine;

public static class MoveRecorder
{
    private static Stack<MoveRecord> moveHistory = new Stack<MoveRecord>();

    public static void RecordMove()
    {
        var record = MoveRestorer.CreateMoveRecord();
        moveHistory.Push(record);
    }

    public static void UndoMove()
    {
        if (moveHistory.Count == 0)
            return;

        MoveRecord record = moveHistory.Pop();
        MoveRestorer.RestoreMoveRecord(record);
        Debug.Log("move undone");
    }
}