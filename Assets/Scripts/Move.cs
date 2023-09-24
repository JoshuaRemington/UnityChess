using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    //0-5 start square
    //6-11 end square
    //12-15 special cases(castling, en passant,etc)
    public ushort moveValue;
		public int startSquare, targetSquare = 0;
	const ushort flagMask = 0b1111000000000000;

	public Move(int startSquare, int targetSquare)
	{
		this.startSquare = startSquare;
		this.targetSquare = targetSquare;
		this.moveValue = (ushort)(startSquare | targetSquare << 6);
	}
	public Move(int startSquare, int targetSquare, int flag)
	{
		this.startSquare = startSquare;
		this.targetSquare = targetSquare;
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public bool Contains(Move[] ar, Move a)
	{
		for(int i = 0; i < ar.Length; i++)
		{
			if(ar[i] == null)
				return false;
			else if(SameMove(ar[i], a))
				return true;
		}
		return false;
	}
    public int flag => moveValue >> 12;

    public static bool SameMove(Move a, Move b) => a.moveValue == b.moveValue;
}
