using System;
using System.Drawing;
using System.Windows.Forms;

namespace AITest
{
	public partial class Form1 : Form
	{
		private SteeringEngine _steering;
		private Graphics _graphics;
		private BufferedGraphics _grafx;
		private BufferedGraphicsContext _context;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			InitDrawingSurface();
			int objects;
			if (!int.TryParse(objectCount.Text, out objects))
				objects = 1;
			_steering = new SteeringEngine(_graphics, mainPanel.ClientSize, objects);
			timer1.Enabled = true;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			ClearScreen();
			_steering.Step();
			_grafx.Render();
		}

		private void ClearScreen()
		{
			_graphics.FillRectangle(Brushes.Black, mainPanel.ClientRectangle);
		}

		private void InitDrawingSurface()
		{
			_context = new BufferedGraphicsContext { MaximumBuffer = mainPanel.ClientSize };
			_grafx = _context.Allocate(mainPanel.CreateGraphics(), new Rectangle(0, 0, mainPanel.ClientSize.Width, mainPanel.ClientSize.Height));
			_graphics = _grafx.Graphics;
			_graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		}
	}
}
