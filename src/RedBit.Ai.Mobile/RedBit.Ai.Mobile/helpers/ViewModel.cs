// source: https://github.com/jamesmontemagno/mvvm-helpers/blob/master/MvvmHelpers/BaseViewModel.cs


using System.Threading.Tasks;
using Xamarin.Forms;

namespace RedBit.Mobile.Core
{
    /// <summary>
    /// Base view model.
    /// </summary>
    public class ViewModel : ObservableObject
    {
        public ViewModel()
        {
        }

        string title = string.Empty;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        string subtitle = string.Empty;

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>The subtitle.</value>
        public string Subtitle
        {
            get => subtitle;
            set => SetProperty(ref subtitle, value);
        }

        string icon = string.Empty;

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon
        {
            get => icon;
            set => SetProperty(ref icon, value);
        }

        bool isBusy;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (SetProperty(ref isBusy, value))
                    IsNotBusy = !isBusy;
            }
        }

        bool isNotBusy = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not busy.
        /// </summary>
        /// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>
        public bool IsNotBusy
        {
            get => isNotBusy;
            set
            {
                if (SetProperty(ref isNotBusy, value))
                    IsBusy = !isNotBusy;
            }
        }

        bool canLoadMore = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can load more.
        /// </summary>
        /// <value><c>true</c> if this instance can load more; otherwise, <c>false</c>.</value>
        public bool CanLoadMore
        {
            get => canLoadMore;
            set => SetProperty(ref canLoadMore, value);
        }

        string header = string.Empty;

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }

        string footer = string.Empty;

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>The footer.</value>
        public string Footer
        {
            get => footer;
            set => SetProperty(ref footer, value);
        }

        #region Alert display helper methods
        public Task DisplayAlert(string title, string message)
        {
            return Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
        }

        public Task<bool> DisplayYesNo(string title, string message)
        {
            return DisplayYesNo(title, message, "Yes", "No");
        }

        public Task<bool> DisplayYesNo(string title, string message, string yes, string no)
        {
            return Application.Current?.MainPage?.DisplayAlert(title, message, yes, no);
        }

        public Task<string> DisplayActionSheet(string title, string destructText = null, params string[] buttons)
        {
            return Application.Current?.MainPage?.DisplayActionSheet(title, "Cancel", destructText, buttons);
        }
        #endregion

        #region Navigation properties for view
        /// <summary>
        /// Gets the current navigation, but be careful it may be null
        /// </summary>
        public virtual INavigation Navigation
        {
            get
            {
                return Application.Current?.MainPage?.Navigation;
            }
        }
        /// <summary>
        /// To be used in the CTOR of a view to determine if navigation back button should be shown
        /// </summary>
        public bool ShowNavigationBackButton { get; set; } = true;
        /// <summary>
        /// To be used in the CTOR of a view to determine if the navigation bar should be shown
        /// </summary>
        public bool ShowNavigationBar { get; set; } = true;
        #endregion

        #region Loading props
        private string _LoadingMessage = "";
        /// <summary>
        /// Sets and gets the LoadingMessage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LoadingMessage
        {
            get => _LoadingMessage;
            set => SetProperty(ref _LoadingMessage, value);
        }
        #endregion
    }
}

