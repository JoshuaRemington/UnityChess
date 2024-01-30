using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This class allows us to create the move objects that help us store information about moves, 
This includes the start position, end position, if the move is an enpassant, it it is a castle, 
a double push pawn move, promotion,
We also have a static null move for certain scenarios in returning functions, error/unknown case
Comparison function for determining if two move objects are equal or not
*/
public class Move
{
		//16 bits in a ushort, so we can have a smaller object using less memory and faster data processing for AI/move generation
    //0-5 start square
    //6-11 end square
    //12-15 special cases(castling, en passant,etc)
 	readonly ushort moveValue;  //holds the values above

		// Flags for the different scenarios that moves can be
	public const int NoFlag = 0b0000;    //nothing special about this move
	public const int EnPassantCaptureFlag = 0b0001;    //we are performing en passant if this move is played
	public const int CastleFlag = 0b0010;  //we are castling
	public const int PawnTwoUpFlag = 0b0011;  //double pawn push

	public const int PromoteToQueenFlag = 0b0100;  //pawn promotion to queeen, below is promotion as well, different end piece
	public const int PromoteToKnightFlag = 0b0101;  
	public const int PromoteToRookFlag = 0b0110;
	public const int PromoteToBishopFlag = 0b0111;

		// Masks
	const ushort startSquareMask = 0b0000000000111111;  //these bits allow us to grab the start square of a move by applying the mask
	const ushort targetSquareMask = 0b0000111111000000;  //Allows us to grab the ending square of a move by applying the mask
	const ushort flagMask = 0b1111000000000000;  //The last 4 bits are for the flags, as seen above, apply and then compare to the flag

	public int enPassantSquare = -1;  //initialize the moves en passant square as -1 and only change it if en passant is possible

	//This constructor is a simple case, where there is no flag, and we only have a start and end square, apply to our moveValue
	public Move(int startSquare, int targetSquare)
	{
		this.moveValue = (ushort)(startSquare | targetSquare << 6);		//Bit manipulation to put into correct slots
	}
	//This constructor does the previous constructor's job and also places the flag bits from the flag parameter
	public Move(int startSquare, int targetSquare, int flag)
	{
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);  
	}

	//special case for enPassant where we assign the enPassantSquare variable with the corresponding location of the Enpassant
	public Move(int startSquare, int targetSquare, int flag, int enPassantSquare)
	{
		this.enPassantSquare = enPassantSquare;  //we over ride the -1 with the square that enpassant takes place
		this.moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public int startSquare => moveValue & startSquareMask;  //arrow function to return the start square of the move
	public int targetSquare => (moveValue & targetSquareMask) >> 6;  //arrow function to return the end square of the move
	//This flag >= PromoteToQueenFlag is true if the int value of flag is greater than or equal to 4, which is true only for promotions
	public bool isPromotion => flag >= PromoteToQueenFlag;  //arrow function to return whether or not the move is a promotion
	public int flag => moveValue >> 12;   //arrow function to return the flag as an int, this is for comparison purposes to 
																				//determine in a move is a certain type of move

	//This contains function is meant to look through our possible moves(the array of moves) and determine if the specified move is in the array(if it is playable)
	public Move Contains(Move[] ar, Move a)
	{
		//For all moves in the array
		for(int i = 0; i < ar.Length; i++)
		{
			//if we reach the part of the array that is null, we know that the move is not in the array, we can return a null move to indicate that
			if(ar[i] == null)  //part of array that is null
				return nullMove;  //return a null move to indicate that the move was not found 
			else if(SameMove(ar[i], a))  //if the moves are the same, the move is in the array, we can return the move in the array(or just the move)
				return ar[i]; //we have found the move in the array
		}
		return nullMove;  //if the array is full but we don't find the move in the array, then not found, return null move
	}
		public static Move nullMove => new Move(0,0);	  //null move, for our case, we say 0 for start and end square which is not possible
		//same move arrow function to determine if two moves are the same, two moves have to be the same if the start and end square are the same
    public static bool SameMove(Move a, Move b) => a.startSquare == b.startSquare && a.targetSquare == b.targetSquare;  
}
