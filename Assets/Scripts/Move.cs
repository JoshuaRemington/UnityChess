using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    //0-5 start square
    //6-11 end square
    //12-15 special cases(castling, en passant,etc)
 	readonly ushort moveValue;

		// Flags
	public const int NoFlag = 0b0000;
	public const int EnPassantCaptureFlag = 0b0001;
	public const int CastleFlag = 0b0010;
	public const int PawnTwoUpFlag = 0b0011;

	public const int PromoteToQueenFlag = 0b0100;
	public const int PromoteToKnightFlag = 0b0101;
	public const int PromoteToRookFlag = 0b0110;
	public const int PromoteToBishopFlag = 0b0111;

		// Masks
	const ushort startSquareMask = 0b0000000000111111;
	const ushort targetSquareMask = 0b0000111111000000;
	const ushort flagMask = 0b1111000000000000;

	public int enPassantSquare = -1;

	public Move(int startSquare, int targetSquare)
	{
		this.moveValue = (ushort)(startSquare | targetSquare << 6);
	}
	public Move(int startSquare, int targetSquare, int flag)
	{
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public Move(int startSquare, int targetSquare, int flag, int enPassantSquare)
	{
		this.enPassantSquare = enPassantSquare;
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public int startSquare => moveValue & startSquareMask;
	public int targetSquare => (moveValue & targetSquareMask) >> 6;
	public bool isPromotion => flag >= PromoteToQueenFlag;
	public int flag => moveValue >> 12;

	public Move Contains(Move[] ar, Move a)
	{
		for(int i = 0; i < ar.Length; i++)
		{
			if(ar[i] == null)
				return nullMove;
			else if(SameMove(ar[i], a))
				return ar[i];
		}
		return nullMove;
	}
		public static Move nullMove => new Move(0,0);	
    public static bool SameMove(Move a, Move b) => a.startSquare == b.startSquare && a.targetSquare == b.targetSquare;
}
