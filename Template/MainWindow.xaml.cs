using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Template;

public partial class MainWindow : Window
{

    private IRenderer renderer = null!;

    public MainWindow() => InitializeComponent();


    private void OnLoad(object sender, RoutedEventArgs e)
    {
        renderer = new ExampleRenderer(this);
        //renderer = new ComputeRenderer(this);

        SizeChanged += OnResize;
        CompositionTarget.Rendering += (object? sender, EventArgs e) => renderer.Render();
    }

    private void OnResize(object sender, SizeChangedEventArgs e)
    {
        int width = (int) content.ActualWidth, height = (int) content.ActualHeight;
        renderer.Resize(width, height);
    }

}
