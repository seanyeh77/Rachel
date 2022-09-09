using Pcsc;
using Pcsc.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace Rachel
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static string UID = "";
        public MainPage()
        {
            this.InitializeComponent();
        }
        SmartCardReader m_cardReader;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {// First try to find a reader that advertises as being NFC
            Windows.Devices.Enumeration.DeviceInformation deviceInfo = await Pcsc.SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Nfc);
            if (deviceInfo == null)
            {
                // If we didn't find an NFC reader, let's see if there's a "generic" reader meaning we're not sure what type it is
                deviceInfo = await Pcsc.SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Any);
            }

            if (deviceInfo == null)
            {
                textblock.Text = "NFC card reader mode not supported on this device";
                //LogMessage("NFC card reader mode not supported on this device", NotifyType.ErrorMessage);
                return;
            }

            if (!deviceInfo.IsEnabled)
            {
                var msgbox = new Windows.UI.Popups.MessageDialog("Your NFC proximity setting is turned off, you will be taken to the NFC proximity control panel to turn it on");
                msgbox.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                await msgbox.ShowAsync();

                //// This URI will navigate the user to the NFC proximity control panel
                //NfcUtils.LaunchNfcProximitySettingsPage();
                return;
            }

            if (m_cardReader == null)
            {
                m_cardReader = await SmartCardReader.FromIdAsync(deviceInfo.Id);
                m_cardReader.CardAdded += cardReader_CardAdded;
                m_cardReader.CardRemoved += cardReader_CardRemoved;
            }

        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Clean up
            if (m_cardReader != null)
            {
                m_cardReader.CardAdded -= cardReader_CardAdded;
                m_cardReader.CardRemoved -= cardReader_CardRemoved;
                m_cardReader = null;
            }

            base.OnNavigatingFrom(e);
        }
        private void cardReader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
        {
            UID = "";
            if (!this.Dispatcher.HasThreadAccess)
            {
                var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { main("null"); });
                return;
            }
        }
        private async void cardReader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            await HandleCard(args.SmartCard);
        }
        private async Task HandleCard(SmartCard card)
        {
            try
            {

                using (SmartCardConnection connection = await card.ConnectAsync())
                {
                    // Try to identify what type of card it was
                    IccDetection cardIdentification = new IccDetection(card);
                    await cardIdentification.DetectCardTypeAync();
                    var apduRes = await connection.TransceiveAsync(new Pcsc.GetUid());
                    UID = BitConverter.ToString(apduRes.ResponseData);
                }

                if (!this.Dispatcher.HasThreadAccess)
                {
                    var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { main(UID); });
                    return;
                }
            }
            catch
            {
                if (!this.Dispatcher.HasThreadAccess)
                {
                    var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { main("null"); });
                    return;
                }
            }
        }
        public async void main(string UID)
        {
            httpclientRachel httpclientRachel = new httpclientRachel();
            string rachelstate = await httpclientRachel.Post(UID);
            switch (rachelstate.Replace("\"",""))
            {
                case "close":
                    textblock.Text = "已關機";
                    break;
                case "UID":
                    textblock.Text = "請前往註冊此卡";
                    break;
                case "isopen":
                    textblock.Text = "雷切機已經開啟";
                    break;
                case "state":
                    textblock.Text = "請前往簽到";
                    break;
                case "error":
                    textblock.Text = "發生錯誤";
                    break;
                case "open":
                    textblock.Text = "已開啟";
                    break;
                case "freeze":
                    textblock.Text = "您已被凍結，請聯絡管理員。";
                    break;
                default:
                    textblock.Text = rachelstate;
                    break;
            }
        }
    }
}
