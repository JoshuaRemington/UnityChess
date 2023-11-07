using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    //0-5 start square
    //6-11 end square
    //12-15 special cases(castling, en passant,etc)
    public ushort moveValue;
		public int startSquare, targetSquare,flag = 0;
	public const int NoFlag = 0b0000;
	public const int EnPassantCaptureFlag = 0b0001;
	public const int CastleFlag = 0b0010;
	public const int PawnTwoUpFlag = 0b0011;
	public const int PromoteToQueenFlag = 0b0100;
	public const int PromoteToKnightFlag = 0b0101;
	public const int PromoteToRookFlag = 0b0110;
	public const int PromoteToBishopFlag = 0b0111;
	const ushort flagMask = 0b1111000000000000;

	public int enPassantSquare = -1;

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
		this.flag = flag;
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public int Contains(Move[] ar, Move a)
	{
		for(int i = 0; i < ar.Length; i++)
		{
			if(ar[i] == null)
				return -1;
			else if(SameMove(ar[i], a))
				return ar[i].flag;
		}
		return -1;
	}

    public static bool SameMove(Move a, Move b) => a.startSquare == b.startSquare && a.targetSquare == b.targetSquare;
}
