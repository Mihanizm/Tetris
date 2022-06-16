using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Tetris
{
	/// <summary>
	/// Главная форма
	/// </summary>
	public partial class MainForm : Form
	{
		private GameField GF;
		private TetrisField Preview;
		private TetrisGame Game;		
		
		private Image PausedImage, GameOverImage;

		public MainForm()
		{
			Game=new TetrisGame(); //новая игра
			Game.Score=0; //баллы
			Game.GameOver=true; //конец игры
			
			
			GF=new GameField(18, 12); //вся площадь для игры 18 х 12
			
			Preview=new TetrisField(4, 4); //поле для тетриса
			Preview.BorderColor=Preview.BackColor; 
			
			Random rnd=new Random(); 
			
			// Типа сплэш
			for(int row=0; row<GF.TilesHeight; row++)
			{
				for(int col=0; col<GF.TilesWidth; col++)
				{
					TileType t=(TileType)rnd.Next(0, 7);
					GF.SetCell(row, col, t);
				}
			}
			
			Game.StateChanged+=new EventHandler(Game_StateChanged);
			
			InitializeComponent();
			
		}

		void Game_StateChanged(object sender, EventArgs e)
		{
			ScoreLabel.Text=Game.Score.ToString(); //очки
			FiguresLabel.Text=Game.FiguresDropped.ToString(); //подсчет количества выпавших фигур
			ElapsedTimeLabel.Text=(DateTime.Now-Game.GameStarted).ToString(); //время с момента старта игры
			Refresh();
		}
		
		private void SetScore(int nscore)
		{
			Game.Score=nscore;
		}
		
		private void NewGame() //начинаем новую игру
		{
			Game=new TetrisGame();
			Game.StateChanged+= new EventHandler(Game_StateChanged);
			SetScore(0); //обнуляем очки
			
			GameTimer.Interval=1000; //ставим таймер для падания фигур
			GameTimer.Enabled=true;
			
			Game.NextFigure=Figure.RandomFigure(); //фигуры выпадают рандомно
			
			GF.Clear(); //обновляем все
			
			Refresh();
		}
		
		private void SetPause(bool enable) //нажатие паузы
		{
			if(Game.GameOver) return; 
			Game.Paused=enable;
			
			GameTimer.Enabled=!enable;
		}
		
		// Игровой цикл
		void GameTimerTick(object sender, EventArgs e)
		{
			if(Game.Paused) return;
			
			GF.DoStep();
			
			if(!GF.IsFigureFalling) 
			{
				//нужно поместить новую фигуру на поле и скрыть полные ряды
				SetScore(Game.Score+GF.RemoveFullRows()*10);
				
				if(!GF.PlaceFigure(Game.NextFigure))
				{
					//игра окончена
					OnGameOver();
				}
				else
				{
					Game.NextFigure=Figure.RandomFigure(); //выбрасываем рандомную фигуру
					Game.FiguresDropped++; //увеличиваем счетчик количества фигур
					FiguresLabel.Text=Game.FiguresDropped.ToString();
					Preview.Clear();
					Preview.SetFigure(Game.NextFigure.MoveTo(1, 1), false);
					
					if(Game.FigureChanged && Game.FiguresDropped%5==0) Game.FigureChanged=false;
					//ускоряем игру при росте количества очков
					if(Game.FiguresDropped%15==0 && Game.Score!=0)
					{
						if(GameTimer.Interval>300)
						{
							GameTimer.Interval-=100;
						}
					}
					
					ShowAdvice(); //здесь показываем подсказки
				}
			}
			ElapsedTimeLabel.Text=(DateTime.Now-Game.GameStarted).ToString(@"mm\:ss"); //получаем время
			
			Refresh();
		}
		
		private void OnGameOver() //если игра окончена
		{
			Game.Over();
			GameTimer.Enabled=false; //отключаем таймер
			MessageBox.Show("Вы проиграли, начните игру заново"); //показываем сообщение
		}
		
		private static string[] Advices=new string[] //массив советов
		{
			"Дождитесь, пока исчезнет индикатор вокруг изображения следующей фигуры, чтобы иметь возможность поменять фигуру!",
			"Используйте клавишу Q, чтобы воспользоваться следующей фигурой",
			"Вместе с количеством сброшенных фигур растёт и скорость игры",
			"Используйте клавишу F3, чтобы поставить игру на паузу",
			"Решили начать новую игру? Нажмите F2, чтобы сделать это немедленно!"
		};
		private void ShowAdvice(int advice) //функция показа советов
		{
			AdviceLabel.Text=Advices[advice];
		}
		private void ShowAdvice() //показываем советы в рандомном порядке
		{
			ShowAdvice(new Random().Next(1, Advices.Length));
		}
		
		// Обработка ввода
		void MainFormKeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress=true; //если произошло нажатие
			
			if(Game.GameOver || Game.Paused) return; //если пауза или игра окончена, то нажатия не обрабатываются
			
			if(e.KeyData==Keys.Left || e.KeyData==Keys.A) //если нажата А или кнопка влево
			{
				GF.MoveLeft(); //двигаем влево
			}
			if(e.KeyData==Keys.Right || e.KeyData==Keys.D) //если нажата кнопка вправо или D
			{
				GF.MoveRight(); //двигаем вправо
			}
			if(e.KeyData==Keys.Up || e.KeyData==Keys.W) //если нажата кнопка вверх или W, т.е опустить фигуру сразу
			{
				if(GF.Drop())
					SetScore(Game.Score+5); //прибавляем +5 баллов за смелость
			}
			if(e.KeyData==Keys.Down || e.KeyData==Keys.S) //если нажата кнопка вниз или S
			{
				if(GF.MoveDown())
					SetScore(Game.Score+1); //опускаем и прибавлем +1 балл за смелость
			}
			if(e.KeyData==Keys.Space) //если нажат пробел, поворачиваем фигуру
			{
				GF.RotateFigure();
			}
			
			if(e.KeyData==Keys.Q) // если нажата Q, меняем на другую фигуру
			{
				if(!Game.FigureChanged && GF.IsFigureFalling) 
				{
					Game.NextFigure=new Figure(GF.ChangeFigure(Game.NextFigure).Type);
					Preview.Clear();
					Preview.SetFigure(Game.NextFigure.MoveTo(1, 1), false);
					Game.FigureChanged=true;
					if(Game.NextFigure==Figure.Zero)
						OnGameOver();
				}
				if(Game.FigureChanged)
				{
					ShowAdvice(0); //показываем совет
				}
			}
			Refresh(); //обновляем
		}

		
		void GameFieldPictureBoxPaint(object sender, PaintEventArgs e)
		{
			GF.Paint(e.Graphics);
			
			if(Game.Paused) //если игра приостановлена, то вставляем картинку паузы
			{
				Rectangle img=new Rectangle((GameFieldPictureBox.Width-PausedImage.Width)/2,
				                            (GameFieldPictureBox.Height-PausedImage.Height)/2,
				                            PausedImage.Width, PausedImage.Height);
				e.Graphics.DrawImage(PausedImage, img);
				return;
			}
			if(Game.GameOver) //если игра окончена то вставляем картинку gameover
			{
				Rectangle img=new Rectangle((GameFieldPictureBox.Width-GameOverImage.Width)/2,
				                            (GameFieldPictureBox.Height-GameOverImage.Height)/2,
				                            GameOverImage.Width, GameOverImage.Height);
				e.Graphics.DrawImage(GameOverImage, img);
			}
		}
		
		void НоваяИграToolStripMenuItemClick(object sender, EventArgs e)
		{
			NewGame(); //новая игра
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			try //загрузка изображений для фигур
			{
				TetrisField.Blue=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImBLUE"));
				TetrisField.Red=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImRED"));
				TetrisField.Green=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImGREEN"));
				TetrisField.LightBlue=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImLBLUE"));
				TetrisField.Purple=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImPURPLE"));
				TetrisField.Yellow=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImYELLOW"));
				TetrisField.Orange=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImORANGE"));
				
				PausedImage=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImPAUSE"));
				GameOverImage=new System.Drawing.Bitmap(GetType().Assembly.GetManifestResourceStream("ImGAMEOVER"));
			}
			catch(Exception ex) //если произошла ошибка при загрузке
			{
				MessageBox.Show(ex.Message, "Ошибка при загрузке изображений!");
			}
		}		
		
		void NextFigurePictureBoxPaint(object sender, PaintEventArgs e)
		{
			Preview.BorderColor=Game.FigureChanged? Color.FromArgb(160, 128, 128) : Preview.BackColor;
			Preview.Paint(e.Graphics);
		}
		
		void TipsCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			GF.ShowTips=TipsCheckBox.Checked;
		}
		
		void ПаузапродолжитьToolStripMenuItemClick(object sender, EventArgs e)
		{
			SetPause(!Game.Paused);
		}
		
		void ВыходToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}
		
		void ОбИгреToolStripMenuItemClick(object sender, EventArgs e)
		{
			new AboutDialog().ShowDialog();
		}

		private void GameFieldPictureBox_Click(object sender, EventArgs e)
		{

		}

		void ПравилаToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(@"help\help.htm");
			}
			catch
			{
				try
				{
					System.Diagnostics.Process.Start(@"help\");
				}
				catch
				{
					MessageBox.Show("Не удалось открыть файл помощи. Попробуйте самостоятельно открыть папку с игрой," +
					                " а в ней - папку help", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
