using System;

class Program
{
	static char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
	static char human = 'X';
	static char ai = 'O';

	static void Main()
	{
		Console.WriteLine("Крестики-нолики (Вы: X, ИИ: O)");
		PrintBoard();

		while (true)
		{
			// Ход человека
			Console.Write("Ваш ход (0-8): ");
			if (!int.TryParse(Console.ReadLine(), out int move) || move < 0 || move > 8 || board[move] != ' ')
			{
				Console.WriteLine("Неверный ход. Выберите свободную клетку от 0 до 8.");
				continue;
			}

			board[move] = human;
			PrintBoard();

			if (CheckWinner() != '\0') break;

			// Ход ИИ
			Console.WriteLine("ИИ думает...");
			int aiMove = FindBestMove();
			board[aiMove] = ai;
			PrintBoard();

			if (CheckWinner() != '\0') break;
		}

		// Результат игры
		char winner = CheckWinner();
		if (winner == 'T') Console.WriteLine("Ничья!");
		else if (winner == human) Console.WriteLine("Вы победили!");
		else Console.WriteLine("ИИ победил!");
	}

	// Вывод доски
	static void PrintBoard()
	{
		Console.WriteLine($"\n {board[0]} | {board[1]} | {board[2]} ");
		Console.WriteLine("---+---+---");
		Console.WriteLine($" {board[3]} | {board[4]} | {board[5]} ");
		Console.WriteLine("---+---+---");
		Console.WriteLine($" {board[6]} | {board[7]} | {board[8]} \n");
	}

	// Проверка победителя (возвращает 'X', 'O', 'T' (ничья) или '\0' (игра продолжается))
	static char CheckWinner()
	{
		int[][] wins = {
			new int[] {0, 1, 2}, new int[] {3, 4, 5}, new int[] {6, 7, 8}, // ряды
            new int[] {0, 3, 6}, new int[] {1, 4, 7}, new int[] {2, 5, 8}, // колонки
            new int[] {0, 4, 8}, new int[] {2, 4, 6}             // диагонали
        };

		foreach (var w in wins)
		{
			if (board[w[0]] != ' ' && board[w[0]] == board[w[1]] && board[w[1]] == board[w[2]])
				return board[w[0]];
		}

		foreach (char c in board)
		{
			if (c == ' ') return '\0'; // еще есть ходы
		}

		return 'T'; // ничья
	}

	// Алгоритм Minimax
	static int Minimax(bool isMaximizing)
	{
		char winner = CheckWinner();
		if (winner == ai) return 10;
		if (winner == human) return -10;
		if (winner == 'T') return 0;

		if (isMaximizing)
		{
			int maxEval = -1000;
			for (int i = 0; i < 9; i++)
			{
				if (board[i] == ' ')
				{
					board[i] = ai;
					int eval = Minimax(false);
					board[i] = ' ';
					maxEval = Math.Max(maxEval, eval);
				}
			}
			return maxEval;
		}
		else
		{
			int minEval = 1000;
			for (int i = 0; i < 9; i++)
			{
				if (board[i] == ' ')
				{
					board[i] = human;
					int eval = Minimax(true);
					board[i] = ' ';
					minEval = Math.Min(minEval, eval);
				}
			}
			return minEval;
		}
	}

	// Поиск лучшего хода для ИИ
	static int FindBestMove()
	{
		int bestScore = -1000;
		int bestMove = -1;

		for (int i = 0; i < 9; i++)
		{
			if (board[i] == ' ')
			{
				board[i] = ai;
				int score = Minimax(false);
				board[i] = ' ';
				if (score > bestScore)
				{
					bestScore = score;
					bestMove = i;
				}
			}
		}
		return bestMove;
	}
}