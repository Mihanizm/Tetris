using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
	public class TetrisGame
	{
		public Figure NextFigure;
		
		private int score;
		public int Score //баллы
		{
			get { return score; }
			set
			{
				score=value;
				OnStateChanged();
			}
		}
		
		private int figDropped;
		public int FiguresDropped //упавшие фигуры
		{
			get { return figDropped; }
			set
			{
				figDropped=value;
				OnStateChanged();
			}
		}
		
		private bool gameOver, paused, figChanged;
		public bool GameOver //конец игры
		{
			get { return gameOver; }
			set
			{
				gameOver=value;
				OnStateChanged();
			}
		}
		public bool Paused //пауза
		{
			get { return paused; }
			set
			{
				if(!paused && value)
				{
					GamePaused=DateTime.Now;
					paused=value;
					OnStateChanged();
				}
				if(paused && !value)
				{
					GameStarted=GameStarted+(DateTime.Now-GamePaused);
					paused=value;
					OnStateChanged();
				}
			}
		}
		public bool FigureChanged //меняет фигуру
		{
			get { return figChanged; }
			set
			{
				figChanged=value;
				OnStateChanged();
			}
		}
		
		public DateTime GameStarted, GamePaused;
		
		
		/// <summary>
		/// Создаёт новый экземпляр TetrisGame, готовый к началу игры
		/// </summary>
		public TetrisGame()
		{
			Score=0; FiguresDropped=0; // ноль очков и упавших фигур
			NextFigure=Figure.RandomFigure(); //следующая фигура рандомно
			GameOver=false; //ничего не нажато
			Paused=false; //ничего не нажато
			FigureChanged =false; //ничего не нажато
			GameStarted = DateTime.Now; //время
		}
		
		/// <summary>
		/// Завершает игру
		/// </summary>
		public void Over()
		{
			GameOver=true;
		}
		
		
		public event EventHandler StateChanged;
		protected virtual void OnStateChanged()
		{
			if (StateChanged != null)
			{
				StateChanged(this, new EventArgs());
			}
		}
	}
}
