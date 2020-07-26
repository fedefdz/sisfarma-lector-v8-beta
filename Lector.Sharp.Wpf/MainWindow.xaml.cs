﻿using Lector.Sharp.Wpf.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MySql.Data.MySqlClient;
using Lector.Sharp.Wpf.Models;
using Lector.Sharp.Wpf.Extensions;
using System.Deployment.Application;
using Lector.Sharp.Wpf.Helpers;
using System.Threading;

namespace Lector.Sharp.Wpf
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _tiempoEspera = 10000;

        /// <summary>
        /// Listener que escucha cada vez que se presiona una tecla.
        /// </summary>
        private LowLevelKeyboardListener _listener;

        private LowLevelWindowsListener _window;

        /// <summary>
        /// Gestiona todos los servicios de SisFarma, como acceso a la
        /// base de datos, lectura de archivos de configuración.
        /// </summary>
        private FarmaService _service;

        /// <summary>
        /// Almacena el valor de las teclas presionadas, específcamente números.
        /// </summary>
        private string _keyData = string.Empty;

        /// <summary>
        /// Window para mostrar información de la base de datos.
        /// Está ventana emerge después de presionar ENTER y las teclas presionadas
        /// previamente forman un código existente en la base de datos.
        /// </summary>
        private BrowserWindow _infoBrowser;

        /// <summary>
        /// Window para mostrar una dirección web. Esta ventana emerge cuando se
        /// presiona SHIFT+F1 y se cierrra al presionar SHIFT+F2
        /// </summary>
        private BrowserWindow _customBrowser;

        /// <summary>
        /// Window para mostrar una dirección web. Esta ventana emerge cuando se
        /// presiona SHIFT+F3 y se cierrra al presionar SHIFT+F2
        /// </summary>
        private BrowserWindow _customBrowserR;

        /// <summary>
        /// Icono de barra de tareas, gestiona la salida del programa
        /// </summary>
        private System.Windows.Forms.NotifyIcon _iconNotification;

        /// <summary>
        /// Devuelve una ventana para mostrar info de la base de datos, si esta ya ce cerró devuelve una nueva
        /// </summary>
        public BrowserWindow InfoBrowser
        {
            get
            {
                if (_infoBrowser.IsClosed)
                {
                    _infoBrowser = new BrowserWindow();
                }
                return _infoBrowser;
            }
        }

        /// <summary>
        /// Devuelve una ventana para mostrar una web específica, si esta ya ce cerró devuelve una nueva
        /// </summary>
        public BrowserWindow CustomBrowser
        {
            get
            {
                if (_customBrowser.IsClosed)
                {
                    _customBrowser = new BrowserWindow();
                }
                return _customBrowser;
            }
        }

        /// <summary>
        /// Devuelve una ventana para mostrar una web específica, si esta ya ce cerró devuelve una nueva
        /// </summary>
        public BrowserWindow CustomBrowserR
        {
            get
            {
                if (_customBrowserR.IsClosed)
                {
                    _customBrowserR = new BrowserWindow();
                }
                return _customBrowserR;
            }
        }



        public MainWindow()
        {
            var updateTimer = new System.Timers.Timer(5000);
            updateTimer.Elapsed += (s, e) =>
            {
                if (Updater.CheckUpdateSyncWithInfo())
                {
                    Updater.UpdateHot();
                    Dispatcher.Invoke(() =>  Application.Current.BeginReStart());
                }

            };
            updateTimer.Start();            

            try
            {
                RegisterStartup();
                SupportHtml5();
                InitializeComponent();
                _service = new FarmaService();
                _listener = new LowLevelKeyboardListener();
                _infoBrowser = new BrowserWindow();
                _customBrowser = new BrowserWindow();
                _customBrowserR = new BrowserWindow();
                _window = new LowLevelWindowsListener();

                // Leemos los archivos de configuración
                _service.LeerFicherosConfiguracion();

                // Setamos el comportamiento de la aplicación al presionar una tecla
                _listener.OnKeyPressed += _listener_OnKeyPressed;

                // Activamos el listener de teclado
                _listener.HookKeyboard();

                // Deshabilitamos HotKey, porque usamos LowLevelKeyboardProc
                //this.RegisterHotKeys();

                _iconNotification = new System.Windows.Forms.NotifyIcon();
                _iconNotification.BalloonTipText = "La Aplicación SisFarma se encuentra ejecutando";
                _iconNotification.BalloonTipTitle = "SisFarma Notificación";
                _iconNotification.Text = "Presione Click para Mostrar";
                _iconNotification.Icon = Lector.Sharp.Wpf.Properties.Resources.Logo;
                _iconNotification.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;

                System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
                System.Windows.Forms.MenuItem notificactionInfoMenu = new System.Windows.Forms.MenuItem("Info");
                notificactionInfoMenu.Click += notificactionInfoMenu_Click;
                System.Windows.Forms.MenuItem notificationQuitMenu = new System.Windows.Forms.MenuItem("Salir");
                notificationQuitMenu.Click += notificationQuitMenu_Click;

                menu.MenuItems.Add(notificactionInfoMenu);
                menu.MenuItems.Add(notificationQuitMenu);
                _iconNotification.ContextMenu = menu;
                _iconNotification.Visible = true;
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        /// <summary>
        /// Registra la aplicación en el Registro de sistema para que arranque junto al sistema.
        /// </summary>
        private void RegisterStartup()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;
            var location = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                @"SisFarma", @"SisFarma Lector.appref-ms");

            RegistryKey reg =
                Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("SisFarma Lector", location);
        }

        private void SupportHtml5()
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true) ??
                              Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
            reg?.SetValue("Lector.Sharp.Wpf.exe", 11001, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Acción para salir del programa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notificationQuitMenu_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();            
        }

        private void notificactionInfoMenu_Click(object sender, EventArgs e)
        {
            try
            {
                var version = $"\n{ApplicationDeployment.CurrentDeployment.CurrentVersion}";
                if (ApplicationDeployment.IsNetworkDeployed)
                    version = $"\n{ApplicationDeployment.CurrentDeployment.CurrentVersion}";

                MessageBox.Show($"SisFarma Applicación{version}\nsisfarma.es");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Desactivamos el listener del teclado
            _listener.UnHookKeyboard();

            // Deshabilitamos HotKey, porque usamos LowLevelKeyboardProc
            this.UnregisteredHotKeys();
        }

        /// <summary>
        /// Precesa el comporatiento de la aplicación al presionarse una tecla.
        /// </summary>
        /// <param name="sender">Listener del teclado</param>
        /// <param name="e">Información de la tecla presionada</param>
        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            try
            {
                if (e.KeyPressed != Key.Enter &&
                    !(_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_CONTROL) && e.KeyPressed == Key.M))
                {
                    #region Low level Keyboard for HotKey

                    // Si presionamos SHIFT + F1
                    if (_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) && e.KeyPressed == Key.F1)
                    {   // Si La ventana de información detallada está abierta la cerramos
                        if (InfoBrowser.IsVisible)
                            CloseWindowBrowser(InfoBrowser);
                        // Abrimos una ventana con la web personalizada.
                        OpenWindowBrowser(CustomBrowser, _service.UrlNavegarCustom, InfoBrowser);
                    }
                    // Si presionamos SHIFT + F3
                    else if (_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) && e.KeyPressed == Key.F3)
                    {
                        // Si La ventana de información detallada está abierta la cerramos
                        if (InfoBrowser.IsVisible)
                            CloseWindowBrowser(InfoBrowser);

                        //Si esta abierta la ventana de custom browser la cerramos
                        if (CustomBrowser.IsVisible)
                            CloseWindowBrowser(CustomBrowser);

                        // Abrimos una ventana con la web personalizada.
                        OpenWindowBrowser(CustomBrowserR, _service.UrlRecomendaciones, InfoBrowser);
                    }
                    // Si presionamos SHIFT + F2
                    else if (_listener.IsHardwareKeyDown(LowLevelKeyboardListener.VirtualKeyStates.VK_SHIFT) && e.KeyPressed == Key.F2)
                    {
                        // Cerramos la ventana con la web personalizada
                        CloseWindowBrowser(CustomBrowser);
                        CloseWindowBrowser(CustomBrowserR);
                    }

                    #endregion Low level Keyboard for HotKey

                    // Almacenamos el valor de la tecla.
                    if (e.KeyPressed.IsDigit(_listener) || e.KeyPressed.IsCharacter(_listener))
                        StoreKey(e.KeyPressed);
                }
                else if (!CustomBrowser.IsActive && !CustomBrowserR.IsActive && !string.IsNullOrEmpty(_keyData))
                {
                    var entryData = _keyData;
                    Task.Run(() => ProccessEnterKey(entryData)).ContinueWith(t =>
                    {
                        if (t.Result)
                        {
                            System.Threading.Thread.Sleep(_tiempoEspera);

                            OpenWindowBrowser(InfoBrowser, _service.UrlNavegar, CustomBrowser);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                    // Limpiamos _keyData para otro proceso.
                    _keyData = string.Empty;
                    // SendKeyEnter();
                }
                else
                {
                    // Siempre que se presiona ENTER se limpia _keyData
                    _keyData = string.Empty;
                }
            }
            catch (MySqlException mysqle)
            {
                //if (!mysqle.Message.Contains("Timeout"))
                //{
                //    MessageBox.Show("Ha ocurrido un error. Comuníquese con el Administrador.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ha ocurrido un error. Comuníquese con el Administrador.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Procesa los _keyData para buscar datos en la base de datos
        /// </summary>
        private bool ProccessEnterKey(string entryData)
        {            
            var entryBarCode = entryData;
            if (QRCode.TryParse(entryData, out QRCode qr))
                entryBarCode = qr.BarCode;

            // Si el valor almacenado en _keyData en numérico y con longitud superior a 4
           // MessageBox.Show(entryBarCode);

            if (long.TryParse(entryBarCode, out long number) && entryBarCode.Length >= 4)
            {
                var lanzarBrowserWindow = false;
                var noEntrar = string.Empty;
                var continuar = false;
                var continuarCodNacional = false;
                var enteredNumbersAux = entryBarCode;

                if (entryBarCode.Length >= 13)
                {
                    entryBarCode = entryBarCode.Substring(entryBarCode.Length - 13);
                    noEntrar = entryBarCode.Substring(0, 4);

                    if (Array.Exists(new[] { "8888", "1010", "1111", "0000", "9902", "9900", "9901", "9903", "9904", "9905", "9906", "9907", "9908", "9909", "9910", "9911", "9912", "9913", "9915", "9916", "9917", "9918", "9919", "9920", "1001", "2014", "2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022", "2023", "2024", "2025", "2026", "2027", "2028", "2029", "2030", "3035", "3036", "3037", "3038", "3038", "3039", "3040","2008","0000" }, x => x.Equals(noEntrar)))
                        continuar = true;
                    else
                    {
                        noEntrar = entryBarCode.Substring(0, 7);
                        if (noEntrar.Equals("8470000"))
                            continuar = true;
                        else
                        {
                            noEntrar = entryBarCode.Substring(0, 3);
                            if (Array.Exists(_service.GetCodigoBarraMedicamentos(), x => x.Equals(noEntrar)))
                                continuarCodNacional = true;
                            else if (Array.Exists(_service.GetCodigoBarraSinonimos(), x => x.Equals(noEntrar)))
                                continuarCodNacional = true;
                        }
                    }
                }

                if (entryBarCode.Length >= 12 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 12);
                    noEntrar = entryBarCode.Substring(0, 4);
                    if ("1111".Equals(noEntrar) || "0000".Equals(noEntrar))
                        continuar = true;
                }

                if (entryBarCode.Length >= 10 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 10);
                    noEntrar = entryBarCode.Substring(0, 4);
                    if ("1111".Equals(noEntrar) || "1930".Equals(noEntrar) || "2008".Equals(noEntrar))
                        continuar = true;
                }

                if (entryBarCode.Length >= 7 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 7);
                    noEntrar = entryBarCode.Substring(0, 4);
                    if (Array.Exists(new[] { "1000", "1001", "1002", "1003" }, x => x.Equals(noEntrar)))
                        continuar = true;
                }

                if (entryBarCode.Length >= 6 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 6);
                    noEntrar = entryBarCode.Substring(0, 3);
                    if (Array.Exists(new[] { "000", "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "100", "101", "102", "103" }, x => x.Equals(noEntrar)))
                        continuar = true;
                }

                if (entryBarCode.Length >= 5 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 5);
                    noEntrar = entryBarCode.Substring(0, 2);
                    if (Array.Exists(new[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }, x => x.Equals(noEntrar)))
                        continuar = true;
                }

                if (entryBarCode.Length >= 4 && !continuar && !continuarCodNacional)
                {
                    entryBarCode = enteredNumbersAux.Substring(entryBarCode.Length - 4);
                    noEntrar = entryBarCode.Substring(0, 2);
                    if (Array.Exists(new[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }, x => x.Equals(noEntrar)))
                        continuar = true;
                }

                if (continuar)
                {
                    var cliente = _service.GetCliente(entryBarCode);
                    if (cliente != null)
                    {
                        lanzarBrowserWindow = true;
                        _service.UrlNavegar = _service.Url.Replace("codigo", cliente.ToString()) + "/" + _service.Mostrador;
                    }
                    else
                    {
                        var trabajador = _service.GetTrabajador(entryBarCode);
                        if (trabajador != null)
                        {
                            lanzarBrowserWindow = true;
                            _service.UrlNavegar = _service.UrlMensajes.Replace("codigo", trabajador.ToString());
                        }
                    }
                }
                else
                {
                    if (continuarCodNacional)
                    {
                        string foundAsociado = null;
                        var codNacional = _service.GetCodigoNacionalSinonimo(entryBarCode.Substring(0, entryBarCode.Length > 12 ? 12 : entryBarCode.Length));
                        if (codNacional == null)
                        {
                            codNacional = _service.GetCodigoNacionalMedicamento(entryBarCode.Substring(0, entryBarCode.Length > 12 ? 12 : entryBarCode.Length));
                            if (codNacional == null)
                                codNacional = Convert.ToInt64(entryBarCode.Substring(3, entryBarCode.Length - 4));
                        }

                        var asociado = _service.GetAsociado(Convert.ToInt64(codNacional));
                        var mostrarVentana = false;
                        if (asociado != null)
                        {
                            mostrarVentana = true;
                            foundAsociado = asociado;
                        }
                        else
                        {
                            var articulo = _service.GetArticulo(Convert.ToInt64(codNacional));
                            if (articulo != null)
                            {
                                mostrarVentana = true;
                                foundAsociado = articulo;
                            }
                            else
                            {
                                var categ = _service.GetCategorizacion();
                                if (categ == null)
                                {
                                    var asociadoCodNacional = _service.GetAnyAsociadoMedicamento(Convert.ToInt64(codNacional));
                                    if (asociadoCodNacional != null)
                                    {
                                        mostrarVentana = true;
                                        foundAsociado = asociadoCodNacional;
                                    }
                                }
                                else
                                {
                                    var asociadoCodNacional = _service.GetAsociadoCategorizacion(Convert.ToInt64(codNacional));
                                    if (asociadoCodNacional == null)
                                    {
                                        asociadoCodNacional = _service.GetAnyAsociadoMedicamento(Convert.ToInt64(codNacional));
                                        if (asociadoCodNacional != null)
                                        {
                                            mostrarVentana = true;
                                            foundAsociado = asociadoCodNacional;
                                        }
                                    }
                                    else
                                    {
                                        mostrarVentana = true;
                                        foundAsociado = asociadoCodNacional;
                                    }
                                }
                            }
                        }

                        if (mostrarVentana && foundAsociado != null)
                        {
                            lanzarBrowserWindow = true;
                            _service.UrlNavegar = _service.Url.Replace("codigo", "cn" + foundAsociado + "/" + _service.Mostrador);
                        }
                    }
                }

                if (lanzarBrowserWindow)
                {
                    // Mostramos el browser con información de la base de datos
                    // Si es proceso de búsqueda en la base de datos es exitoso
                    // mostramos los resultados
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Almacena el valor de la tacla en _keyData
        /// </summary>
        /// <param name="key">Tecla presionada</param>
        private void StoreKey(Key key)
        {
            //if (_keyData.Length > 50)
            //    _keyData = _keyData.Substring(_keyData.Length - 20 - 1);
            var kc = new KeyConverter();
            // Key.NumPad# se convierte en 'NumPad#' por lo cual lo eliminamos
            _keyData += kc.ConvertToString(key)?.Replace("NumPad", string.Empty);
        }

        /// <summary>
        /// Cierra una ventana que contiene un browser
        /// </summary>
        /// <param name="browser"></param>
        private void CloseWindowBrowser(BrowserWindow browser)
        {
            browser.Hide();
        }

        /// <summary>
        /// Abre una ventana que contiene un browser
        /// </summary>
        /// <param name="browser">Ventana con un browser</param>
        private void OpenWindowBrowser(BrowserWindow browser, string url, BrowserWindow hidden)
        {
            hidden.Topmost = false;
            browser.Topmost = true;
            hidden.Topmost = true;
            browser.Browser.Navigate(url);
            browser.Visibility = Visibility.Visible;
            browser.WindowState = WindowState.Maximized;
            browser.Show();
            browser.Activate();
        }

        /// <summary>
        /// Simula presionar la tecla ENTER
        /// </summary>
        public static void SendKeyEnter()
        {
            // Utilizar SendWait para compatibilidad con WPF
            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
        }

        public static void SendKeyA()
        {
            // Utilizar SendWait para compatibilidad con WPF
            System.Windows.Forms.SendKeys.SendWait("{A}");
        }

        #region HotKeys

        private const int WM_HOTKEY = 0x0312;
        private const UInt32 MOD_SHIFT = 0x0004;
        private const string WM_ATOMNAME_SHIFT_F1 = "SFRM_LECTOR_SHIFT_F1";
        private const string WM_ATOMNAME_SHIFT_F2 = "SFRM_LECTOR_SHIFT_F2";
        private ushort kbShiftF1;
        private ushort kbShiftF2;
        private IntPtr CurrentProcess;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ushort GlobalAddAtom(string atomName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        private void RegisterHotKeys()
        {
            //var moudle = Process.GetCurrentProcess().MainModule.ModuleName;
            //CurrentProcess = GetModuleHandle(moudle);
            CurrentProcess = Process.GetCurrentProcess().MainWindowHandle;

            kbShiftF1 = GlobalAddAtom(WM_ATOMNAME_SHIFT_F1);
            kbShiftF2 = GlobalAddAtom(WM_ATOMNAME_SHIFT_F2);

            var res = RegisterHotKey(CurrentProcess, kbShiftF1, MOD_SHIFT, (UInt32)LowLevelKeyboardListener.VirtualKeyStates.VK_F1);
            res = RegisterHotKey(CurrentProcess, kbShiftF2, MOD_SHIFT, (UInt32)LowLevelKeyboardListener.VirtualKeyStates.VK_F2);

            ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
        }

        public void UnregisteredHotKeys()
        {
            GlobalDeleteAtom(kbShiftF1);
            GlobalDeleteAtom(kbShiftF2);
            var res = UnregisterHotKey(CurrentProcess, kbShiftF1);
            res = UnregisterHotKey(CurrentProcess, kbShiftF2);
        }

        private void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                if ((int)msg.wParam == kbShiftF1)
                {
                    if (InfoBrowser.IsVisible) CloseWindowBrowser(InfoBrowser);
                    if (!CustomBrowser.IsVisible) OpenWindowBrowser(CustomBrowser, _service.UrlNavegarCustom, InfoBrowser);
                }
                else if ((int)msg.wParam == kbShiftF2)
                {
                    CloseWindowBrowser(CustomBrowser);
                }
            }
        }

        #endregion HotKeys
    }
}