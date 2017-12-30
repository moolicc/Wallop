
GetController = new Func<Controller>(() =>
{
    return new control();
});

GetOptionsControl = new Func<LayerSettings, Panel>((settings) =>
{
    var textBox = new TextBox();
    // 
    // textBox1
    // 
    textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
    | System.Windows.Forms.AnchorStyles.Right)));
    textBox.Location = new System.Drawing.Point(3, 3);
    textBox.Size = new System.Drawing.Size(197, 20);
    textBox.Text = settings["imagefile"];

    // 
    // button1
    // 
    var button = new Button
    {
        Location = new System.Drawing.Point(206, 3),
        Size = new System.Drawing.Size(75, 23),
        UseVisualStyleBackColor = true,
        Text = "Browse"
    };
    button.Click += (s, e) =>
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Title = "Select Image";
        fileDialog.FileName = textBox.Text;
        fileDialog.Filter = "Image files (*.jpg *.jpeg *.png) | *.jpg; *.jpeg; *.png";

        if(fileDialog.ShowDialog() == DialogResult.OK)
        {
            textBox.Text = fileDialog.FileName;
            settings["imagefile"] = fileDialog.FileName;
        }
    };

    // 
    // tableLayoutPanel1
    // 
    var layoutPanel = new TableLayoutPanel();

    layoutPanel.AutoSize = true;
    layoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
    layoutPanel.ColumnCount = 2;
    layoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
    layoutPanel.RowCount = 1;

    layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
    layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
    layoutPanel.Controls.Add(button, 1, 0);
    layoutPanel.Controls.Add(textBox, 0, 0);
    layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

    return layoutPanel;
});

class control : Controller
{
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    
    public override void Setup()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        using (var fs = new FileStream(LayerSettings["imagefile"], FileMode.Open))
        {
            _texture = Texture2D.FromStream(GraphicsDevice, fs);
        }
    }
    
    public override void EnabledChanged()
    {
    }
    
    public override void Dispose()
    {
    }
    
    public override void Update(GameTime gameTime)
    {
    }
    
    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, new Rectangle(0, 0, RenderTarget.Width, RenderTarget.Height), Color.White);
        _spriteBatch.End();
    }
}