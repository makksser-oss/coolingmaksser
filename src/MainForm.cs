using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Microsoft.Win32;

namespace LovingCoolStudio;

public sealed class MainForm : Form {
    readonly PanelDevice panel=new(); readonly SensorService sensors=new();
    readonly System.Windows.Forms.Timer timer=new(){Interval=400}; readonly NotifyIcon tray=new();
    readonly Color bg=Color.FromArgb(7,11,20), nav=Color.FromArgb(12,18,31), card=Color.FromArgb(17,25,42), card2=Color.FromArgb(23,34,56);
    readonly Color cyan=Color.FromArgb(30,215,255), purple=Color.FromArgb(151,62,255), fg=Color.FromArgb(241,245,255), muted=Color.FromArgb(145,160,190);
    readonly CanvasPanel canvas=new(){Dock=DockStyle.Fill,Margin=new Padding(16),BackColor=Color.FromArgb(6,10,19)};
    readonly Label deviceState=new(){AutoSize=true}; readonly FlowLayoutPanel sensorGrid=new(){Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,Padding=new Padding(4)};
    readonly FlowLayoutPanel layers=new(){Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};
    readonly ComboBox sensorPicker=new(){DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Top};
    readonly NumericUpDown fontSize=new(){Minimum=10,Maximum=100,Value=28,Dock=DockStyle.Top};
    readonly TextBox widgetText=new(){Dock=DockStyle.Top};
    readonly List<Widget> widgets=[]; Dictionary<string,float> snapshot=[];
    Bitmap? background; Widget? selected; bool mirror; int rotation=90, brightness=80;

    public MainForm(){
        Text="LOVINGCOOL Studio"; StartPosition=FormStartPosition.CenterScreen; WindowState=FormWindowState.Maximized;
        MinimumSize=new(1280,760); BackColor=bg; ForeColor=fg; Font=new("Segoe UI",10); DoubleBuffered=true;
        try{Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);}catch{}
        Build(); Seed(); SetupTray(); timer.Tick+=(s,e)=>RenderTick(); timer.Start();
        FormClosing+=(s,e)=>{timer.Stop();panel.Dispose();sensors.Dispose();tray.Visible=false;};
    }
    Panel Card()=>new(){BackColor=card,Padding=new Padding(14),Margin=new Padding(8)};
    Label L(string t,int size=10,bool bold=false,Color? c=null)=>new(){Text=t,AutoSize=true,ForeColor=c??(bold?fg:muted),Font=new("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(4,5,4,5)};
    Button B(string t,EventHandler click,bool hot=false){var b=new Button{Text=t,Height=42,Dock=DockStyle.Top,FlatStyle=FlatStyle.Flat,BackColor=hot?Color.FromArgb(72,67,224):card2,ForeColor=fg,Font=new("Segoe UI",10,FontStyle.Bold),Cursor=Cursors.Hand,Margin=new Padding(0,4,0,4)};b.FlatAppearance.BorderSize=0;b.Click+=click;return b;}
    void Build(){
        var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,BackColor=bg,Padding=new Padding(0)};
        root.ColumnStyles.Add(new(SizeType.Absolute,235)); root.ColumnStyles.Add(new(SizeType.Percent,100)); root.ColumnStyles.Add(new(SizeType.Absolute,300)); root.ColumnStyles.Add(new(SizeType.Absolute,300)); Controls.Add(root);
        BuildNav(root); BuildCenter(root); BuildLibrary(root); BuildProps(root);
    }
    void BuildNav(TableLayoutPanel root){
        var p=new Panel{Dock=DockStyle.Fill,BackColor=nav,Padding=new Padding(18)};root.Controls.Add(p,0,0);
        var logo=new Label{Text="◈  LOVINGCOOL\n     STUDIO",Dock=DockStyle.Top,Height=72,ForeColor=fg,Font=new("Segoe UI",18,FontStyle.Bold)};p.Controls.Add(logo);
        var bottom=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=210,FlowDirection=FlowDirection.TopDown,WrapContents=false};p.Controls.Add(bottom);
        deviceState.Text="●  LOVINGCOOL 6 PRO\n    480 × 480  •  отключено";deviceState.ForeColor=muted;deviceState.Width=195;deviceState.Height=50;bottom.Controls.Add(deviceState);
        var br=L("ЯРКОСТЬ",9,true);bottom.Controls.Add(br);var slider=new TrackBar{Width=190,Minimum=5,Maximum=100,Value=80,TickStyle=TickStyle.None};slider.ValueChanged+=(s,e)=>{brightness=slider.Value;if(panel.Connected)panel.SetBrightness(brightness);};bottom.Controls.Add(slider);
        bottom.Controls.Add(B("Подключить",(_,_)=>Connect(),true)); bottom.Controls.Add(B("Свернуть в трей",(_,_)=>Hide()));
        var menu=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(0,20,0,0)};p.Controls.Add(menu);
        foreach(var (icon,title,action) in new (string,string,Action)[]{("⌂","Главная",()=>mirror=false),("▦","Редактор экрана",()=>mirror=false),("⌁","Мониторинг",()=>ShowSensors()),("▶","Медиа",()=>ChooseBackground()),("▣","Зеркалирование",()=>mirror=true),("◫","Доп. экран (Beta)",()=>MessageBox.Show("Настоящий расширенный дисплей требует Windows IDD-драйвер. Зеркалирование уже работает.","LOVINGCOOL Studio")),("◎","Профили",()=>SaveProfile()),("⚙","Настройки",()=>Settings())}){
            var b=B($"{icon}    {title}",(_,_)=>action());b.Width=195;b.TextAlign=ContentAlignment.MiddleLeft;menu.Controls.Add(b);
        }
    }
    void BuildCenter(TableLayoutPanel root){
        var p=new Panel{Dock=DockStyle.Fill,Padding=new Padding(18),BackColor=bg};root.Controls.Add(p,1,0);
        var head=new Panel{Dock=DockStyle.Top,Height=74};p.Controls.Add(head);
        head.Controls.Add(new Label{Text="Редактор экрана",AutoSize=true,Location=new(4,4),ForeColor=fg,Font=new("Segoe UI",22,FontStyle.Bold)});
        head.Controls.Add(new Label{Text="LOVINGCOOL 6 PRO  •  480 × 480  •  живой предпросмотр",AutoSize=true,Location=new(6,42),ForeColor=muted});
        var presets=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=145,BackColor=card,Padding=new Padding(12),WrapContents=false,AutoScroll=true};p.Controls.Add(presets);
        foreach(var name in new[]{"NEON","CYBER","MINIMAL","SYSTEM","GAMING","CLOCK"}){
            var b=new Button{Text=name,Width=120,Height=105,FlatStyle=FlatStyle.Flat,BackColor=card2,ForeColor=fg,Font=new("Segoe UI",10,FontStyle.Bold),Margin=new Padding(6)};b.FlatAppearance.BorderColor=Color.FromArgb(55,75,115);b.Click+=(_,_)=>ApplyPreset(name);presets.Controls.Add(b);
        }
        var stage=Card();stage.Dock=DockStyle.Fill;p.Controls.Add(stage);stage.BringToFront();
        var frame=new Panel{Width=550,Height=550,Anchor=AnchorStyles.None,BackColor=Color.FromArgb(10,15,28),Padding=new Padding(25)};
        stage.Controls.Add(frame);stage.Resize+=(_,_)=>frame.Location=new((stage.ClientSize.Width-frame.Width)/2,(stage.ClientSize.Height-frame.Height)/2);
        frame.Controls.Add(canvas);canvas.Paint+=PaintCanvas;canvas.MouseDown+=CanvasDown;canvas.MouseMove+=CanvasMove;canvas.MouseUp+=(_,_)=>selected=null;
    }
    void BuildLibrary(TableLayoutPanel root){
        var p=Card();p.Dock=DockStyle.Fill;root.Controls.Add(p,2,0);
        var title=L("Добавить элемент",15,true);title.Dock=DockStyle.Top;p.Controls.Add(title);
        var tabs=new FlowLayoutPanel{Dock=DockStyle.Top,Height=205,WrapContents=true,Padding=new Padding(0,8,0,0)};p.Controls.Add(tabs);
        foreach(var x in new[]{("T","Текст","text"),("◷","Часы","clock"),("▧","Изображение","image"),("▶","Видео / GIF","media"),("♨","Температура","temp"),("▥","Загрузка","load"),("⌁","Частота","clockSensor"),("◉","Мощность","power"),("◌","Вентилятор","fan"),("▤","Память","memory"),("⌁","Сеть","network"),("FPS","FPS","fps")}){
            var b=new Button{Text=x.Item1+"\n"+x.Item2,Width=82,Height=58,FlatStyle=FlatStyle.Flat,BackColor=card2,ForeColor=fg,Margin=new Padding(3)};b.FlatAppearance.BorderSize=0;var kind=x.Item3;b.Click+=(_,_)=>AddKind(kind);tabs.Controls.Add(b);
        }
        var sh=L("ДАТЧИКИ ПК",9,true);sh.Dock=DockStyle.Top;p.Controls.Add(sh);sensorPicker.BackColor=card2;sensorPicker.ForeColor=fg;p.Controls.Add(sensorPicker);
        var add=B("+ Добавить выбранный датчик",(_,_)=>AddSelectedSensor(),true);p.Controls.Add(add);
        sensorGrid.BackColor=card;p.Controls.Add(sensorGrid);sensorGrid.BringToFront();RefreshSensors();
    }
    void BuildProps(TableLayoutPanel root){
        var p=Card();p.Dock=DockStyle.Fill;root.Controls.Add(p,3,0);
        var title=L("Свойства элемента",15,true);title.Dock=DockStyle.Top;p.Controls.Add(title);
        var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=110,FlowDirection=FlowDirection.TopDown,WrapContents=false};p.Controls.Add(actions);actions.Controls.Add(B("Применить",(_,_)=>ApplyProps(),true));actions.Controls.Add(B("Удалить элемент",(_,_)=>DeleteSelected()));
        var body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(2,12,2,2)};p.Controls.Add(body);body.BringToFront();
        body.Controls.Add(L("Текст / шаблон",9,true));widgetText.Width=250;body.Controls.Add(widgetText);body.Controls.Add(L("Размер шрифта",9,true));fontSize.Width=250;body.Controls.Add(fontSize);
        body.Controls.Add(L("ОРИЕНТАЦИЯ ДИСПЛЕЯ",9,true));var rot=new ComboBox{Width=250,DropDownStyle=ComboBoxStyle.DropDownList,BackColor=card2,ForeColor=fg};rot.Items.AddRange(["90° влево (по умолчанию)","0°","180°","90° вправо"]);rot.SelectedIndex=0;rot.SelectedIndexChanged+=(_,_)=>rotation=rot.SelectedIndex switch{0=>90,1=>0,2=>180,_=>270};body.Controls.Add(rot);
        body.Controls.Add(L("СЛОИ",9,true));layers.Width=250;layers.Height=250;body.Controls.Add(layers);body.Controls.Add(B("Фоновое изображение…",(_,_)=>ChooseBackground()));
    }
    void Seed(){widgets.Add(new("clock","{time}",42,370,54));widgets.Add(new("sensor","CPU  {value}",32,40,28){Hint="CPU"});widgets.Add(new("sensor","GPU  {value}",285,40,28){Hint="GPU"});RefreshLayers();}
    void Connect(){try{panel.Connect();panel.SetBrightness(brightness);deviceState.Text=$"●  LOVINGCOOL 6 PRO\n    {panel.Width} × {panel.Height} • {panel.PortName} • FW {panel.Firmware}";deviceState.ForeColor=Color.FromArgb(67,230,155);}catch(Exception ex){MessageBox.Show(ex.Message,"LOVINGCOOL Studio",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    void RenderTick(){try{snapshot=sensors.Snapshot();canvas.Invalidate();if(panel.Connected){using var b=Compose();panel.Send(b);}}catch{}}
    Bitmap Compose(){Bitmap b;if(mirror){var r=Screen.PrimaryScreen!.Bounds;b=new(r.Width,r.Height);using(var g=Graphics.FromImage(b))g.CopyFromScreen(r.Location,Point.Empty,r.Size);var q=Fit(b);b.Dispose();b=q;}else{b=background!=null?new(background):new(480,480);using var g=Graphics.FromImage(b);if(background==null){using var grad=new LinearGradientBrush(new Rectangle(0,0,480,480),Color.FromArgb(5,15,35),Color.FromArgb(32,8,55),35);g.FillRectangle(grad,0,0,480,480);using var pen=new Pen(Color.FromArgb(80,20,210,255),2);g.DrawEllipse(pen,45,80,170,170);pen.Color=Color.FromArgb(90,180,40,255);g.DrawEllipse(pen,265,80,170,170);}g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;foreach(var w in widgets)Draw(g,w);}if(rotation!=0)b.RotateFlip(rotation switch{90=>RotateFlipType.Rotate270FlipNone,180=>RotateFlipType.Rotate180FlipNone,_=>RotateFlipType.Rotate90FlipNone});return b;}
    void Draw(Graphics g,Widget w){var s=w.Text.Replace("{time}",DateTime.Now.ToString("HH:mm")).Replace("{date}",DateTime.Now.ToString("dd.MM.yyyy"));if(w.Kind=="sensor"){var key=w.SensorKey??snapshot.Keys.FirstOrDefault(k=>k.Contains(w.Hint??"",StringComparison.OrdinalIgnoreCase));s=s.Replace("{value}",key!=null&&snapshot.TryGetValue(key,out var v)?Format(key,v):"--");}using var f=new Font("Segoe UI",w.Size,FontStyle.Bold,GraphicsUnit.Pixel);using var sh=new SolidBrush(Color.FromArgb(140,0,0,0));using var brush=new SolidBrush(w.Color);g.DrawString(s,f,sh,w.X+2,w.Y+2);g.DrawString(s,f,brush,w.X,w.Y);}
    void PaintCanvas(object? s,PaintEventArgs e){using var b=Compose();e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;e.Graphics.DrawImage(b,new Rectangle((canvas.Width-480)/2,(canvas.Height-480)/2,480,480));}
    static Bitmap Fit(Bitmap src){var d=new Bitmap(480,480);using var g=Graphics.FromImage(d);g.InterpolationMode=InterpolationMode.HighQualityBicubic;float z=Math.Max(480f/src.Width,480f/src.Height);var w=(int)(src.Width*z);var h=(int)(src.Height*z);g.DrawImage(src,(480-w)/2,(480-h)/2,w,h);return d;}
    void RefreshSensors(){snapshot=sensors.Snapshot();sensorPicker.Items.Clear();sensorGrid.Controls.Clear();foreach(var kv in snapshot.OrderBy(x=>x.Key)){sensorPicker.Items.Add(kv.Key);if(sensorGrid.Controls.Count<12){var l=new Label{Text=Short(kv.Key)+"   "+Format(kv.Key,kv.Value),Width=250,Height=32,ForeColor=muted,BackColor=card2,Padding=new Padding(6),Margin=new Padding(2)};sensorGrid.Controls.Add(l);}}if(sensorPicker.Items.Count>0)sensorPicker.SelectedIndex=0;}
    void ShowSensors(){RefreshSensors();}
    void AddSelectedSensor(){if(sensorPicker.SelectedItem is not string k)return;widgets.Add(new("sensor",Short(k)+"  {value}",30,300,24){SensorKey=k});RefreshLayers();}
    void AddKind(string kind){if(kind=="text")widgets.Add(new("text","LOVINGCOOL",40,300,30));else if(kind=="clock")widgets.Add(new("clock","{time}",40,300,46));else{var hint=kind switch{"temp"=>"Temperature","load"=>"Load","clockSensor"=>"Clock","power"=>"Power","fan"=>"Fan","memory"=>"Memory","network"=>"Network",_=>kind};var k=snapshot.Keys.FirstOrDefault(x=>x.Contains(hint,StringComparison.OrdinalIgnoreCase));widgets.Add(new("sensor",hint.ToUpper()+"  {value}",40,300,24){SensorKey=k,Hint=hint});}RefreshLayers();}
    void RefreshLayers(){layers.Controls.Clear();foreach(var w in widgets){var b=B("≡  "+w.Text,(_,_)=>Select(w));b.Width=240;b.Height=36;layers.Controls.Add(b);}}
    void Select(Widget w){selected=w;widgetText.Text=w.Text;fontSize.Value=Math.Clamp(w.Size,10,100);}
    void ApplyProps(){if(selected==null)return;selected.Text=widgetText.Text;selected.Size=(int)fontSize.Value;RefreshLayers();canvas.Invalidate();}
    void DeleteSelected(){if(selected==null)return;widgets.Remove(selected);selected=null;RefreshLayers();}
    Widget? drag;Point offset;void CanvasDown(object? s,MouseEventArgs e){int ox=(canvas.Width-480)/2,oy=(canvas.Height-480)/2,x=e.X-ox,y=e.Y-oy;drag=widgets.LastOrDefault(w=>x>=w.X&&x<w.X+260&&y>=w.Y&&y<w.Y+w.Size+18);if(drag!=null){offset=new(x-drag.X,y-drag.Y);Select(drag);}}
    void CanvasMove(object? s,MouseEventArgs e){if(e.Button!=MouseButtons.Left||drag==null)return;int ox=(canvas.Width-480)/2,oy=(canvas.Height-480)/2;drag.X=Math.Clamp(e.X-ox-offset.X,0,450);drag.Y=Math.Clamp(e.Y-oy-offset.Y,0,450);canvas.Invalidate();}
    void ChooseBackground(){using var d=new OpenFileDialog{Filter="Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.webp"};if(d.ShowDialog()==DialogResult.OK){using var i=Image.FromFile(d.FileName);background=Fit(new Bitmap(i));}}
    void ApplyPreset(string n){widgets.Clear();if(n=="MINIMAL"){widgets.Add(new("clock","{time}",120,175,74));widgets.Add(new("sensor","CPU  {value}",140,285,25){Hint="CPU"});}else{Seed();}RefreshLayers();}
    void SaveProfile(){MessageBox.Show("Профили будут сохранены в следующем build вместе с медиатекой видео/GIF.","LOVINGCOOL Studio");}
    void Settings(){try{using var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true)!;const string n="LOVINGCOOL Studio";if(k.GetValue(n)==null){k.SetValue(n,$"\"{Application.ExecutablePath}\"");MessageBox.Show("Автозапуск включён.");}else{k.DeleteValue(n,false);MessageBox.Show("Автозапуск выключен.");}}catch(Exception ex){MessageBox.Show(ex.Message);}}
    void SetupTray(){tray.Text="LOVINGCOOL Studio";tray.Icon=Icon??SystemIcons.Application;tray.Visible=true;var m=new ContextMenuStrip();m.Items.Add("Открыть",null,(_,_)=>{Show();WindowState=FormWindowState.Maximized;});m.Items.Add("Выход",null,(_,_)=>Application.Exit());tray.ContextMenuStrip=m;tray.DoubleClick+=(_,_)=>Show();}
    static string Short(string k){var a=k.Split('•',StringSplitOptions.TrimEntries);return a.Length>0?a[^1]:k;} static string Format(string k,float v){if(k.Contains("Temperature"))return $"{v:0}°C";if(k.Contains("Load"))return $"{v:0}%";if(k.Contains("Clock"))return $"{v:0} MHz";if(k.Contains("Power"))return $"{v:0.0} W";if(k.Contains("Fan"))return $"{v:0} RPM";return $"{v:0.0}";}
    sealed class Widget{public string Kind,Text;public int X,Y,Size;public string? SensorKey,Hint;public Color Color=Color.White;public Widget(string k,string t,int x,int y,int s){Kind=k;Text=t;X=x;Y=y;Size=s;}}
    sealed class CanvasPanel:Panel{public CanvasPanel(){DoubleBuffered=true;ResizeRedraw=true;}}
}