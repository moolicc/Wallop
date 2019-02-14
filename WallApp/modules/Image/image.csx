
GetController = new Func<Controller>(() =>
{
    return new control();
});

GetSettingsController = new Func<SettingsController>(() =>
{
    return new SettingsControl();
});

class SettingsControl : SettingsController
{
    private TextBox textBox;

    public override Control GetSettingsControl()
    {
        textBox = new TextBox();
        // 
        // textBox1
        // 
        textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        textBox.Location = new System.Drawing.Point(3, 3);
        textBox.Size = new System.Drawing.Size(197, 20);
        textBox.Text = Settings["imagefile"];

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
        button.Click += BrowseClicked;

        //
        // label1
        //
        var label = new Label();
        label.Text = "Selected Image";

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
        layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 100F));
        layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        layoutPanel.Controls.Add(label, 0, 0);
        layoutPanel.Controls.Add(button, 1, 1);
        layoutPanel.Controls.Add(textBox, 0, 1);

        return layoutPanel;
    }

    public override string ApplyClicked()
    {
        if (string.IsNullOrWhiteSpace(textBox.Text) || !System.IO.File.Exists(textBox.Text))
        {
            return "Please select a valid image file.";
        }
        Settings["imagefile"] = textBox.Text;
        return "";
    }

    public override void CancelClicked()
    {

    }

    private void BrowseClicked(object sender, EventArgs e)
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Title = "Select Image";
        fileDialog.FileName = textBox.Text;
        fileDialog.Filter = "Image files (*.jpg *.jpeg *.png) | *.jpg; *.jpeg; *.png";

        if (fileDialog.ShowDialog() == DialogResult.OK)
        {
            textBox.Text = fileDialog.FileName;
        }
    }
}

class control : Controller
{
    private Rectangle _drawLocation;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    
    public override void Setup()
    {
        _spriteBatch = new SpriteBatch(Rendering.GraphicsDevice);
        using (var fs = new FileStream(Settings["imagefile"], FileMode.Open))
        {
            _texture = Texture2D.FromStream(Rendering.GraphicsDevice, fs);
        }
        _drawLocation = new Microsoft.Xna.Framework.Rectangle(0, 0, Rendering.ActualWidth, Rendering.ActualHeight);
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
        _spriteBatch.Draw(_texture, _drawLocation, Color.White);
        _spriteBatch.End();
    }
}