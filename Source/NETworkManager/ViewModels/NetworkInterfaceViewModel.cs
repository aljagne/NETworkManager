﻿using System.Collections.Generic;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Models.Network;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using NETworkManager.Views;
using NETworkManager.Utilities;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        private const string tagIdentifier = "tag=";
        
        private bool _isLoading = true;

        public bool IsAdmin
        {
            get { return ConfigurationManager.Current.IsAdmin; }
        }

        private bool _isNetworkInteraceLoading;
        public bool IsNetworkInterfaceLoading
        {
            get { return _isNetworkInteraceLoading; }
            set
            {
                if (value == _isNetworkInteraceLoading)
                    return;

                _isNetworkInteraceLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _canConfigure;
        public bool CanConfigure
        {
            get { return _canConfigure; }
            set
            {
                if (value == _canConfigure)
                    return;

                _canConfigure = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigurationRunning;
        public bool IsConfigurationRunning
        {
            get { return _isConfigurationRunning; }
            set
            {
                if (value == _isConfigurationRunning)
                    return;

                _isConfigurationRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool ShowCurrentApplicationTitle
        {
            get { return SettingsManager.Current.Window_ShowCurrentApplicationTitle; }
        }

        #region NetworkInterfaces, SelectedNetworkInterface
        private List<NetworkInterfaceInfo> _networkInterfaces;
        public List<NetworkInterfaceInfo> NetworkInterfaces
        {
            get { return _networkInterfaces; }
            set
            {
                if (value == _networkInterfaces)
                    return;

                _networkInterfaces = value;
                OnPropertyChanged();
            }
        }

        private NetworkInterfaceInfo _selectedNetworkInterface;
        public NetworkInterfaceInfo SelectedNetworkInterface
        {
            get { return _selectedNetworkInterface; }
            set
            {
                if (value == _selectedNetworkInterface)
                    return;

                if (value != null)
                {
                    if (!_isLoading)
                        SettingsManager.Current.NetworkInterface_SelectedInterfaceId = value.Id;

                    // Details
                    DetailsName = value.Name;
                    DetailsDescription = value.Description;
                    DetailsType = value.Type;
                    DetailsPhysicalAddress = value.PhysicalAddress;
                    DetailsStatus = value.Status;
                    DetailsSpeed = value.Speed;
                    DetailsIPv4Address = value.IPv4Address;
                    DetailsSubnetmask = value.Subnetmask;
                    DetailsIPv4Gateway = value.IPv4Gateway;
                    DetailsIPv4DhcpEnabled = value.DhcpEnabled;
                    DetailsIPv4DhcpServer = value.DhcpServer;
                    DetailsDhcpLeaseObtained = value.DhcpLeaseObtained;
                    DetailsDhcpLeaseExpires = value.DhcpLeaseExpires;
                    DetailsIPv6AddressLinkLocal = value.IPv6AddressLinkLocal;
                    DetailsIPv6Address = value.IPv6Address;
                    DetailsIPv6Gateway = value.IPv6Gateway;
                    DetailsDNSAutoconfigurationEnabled = value.DNSAutoconfigurationEnabled;
                    DetailsDNSSuffix = value.DNSSuffix;
                    DetailsDNSServer = value.DNSServer;

                    // Configuration
                    if (value.DhcpEnabled)
                    {
                        ConfigEnableDynamicIPAddress = true;
                    }
                    else
                    {
                        ConfigEnableStaticIPAddress = true;
                        ConfigIPAddress = (value != null) ? value.IPv4Address.FirstOrDefault().ToString() : string.Empty;
                        ConfigSubnetmaskOrCidr = (value.Subnetmask != null) ? value.Subnetmask.FirstOrDefault().ToString() : string.Empty;
                        ConfigGateway = (value.IPv4Gateway?.Any() == true) ? value.IPv4Gateway.FirstOrDefault().ToString() : string.Empty;
                    }

                    if (value.DNSAutoconfigurationEnabled)
                    {
                        ConfigEnableDynamicDNS = true;
                    }
                    else
                    {
                        ConfigEnableStaticDNS = true;

                        List<IPAddress> DNSServers = value.DNSServer.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
                        ConfigPrimaryDNSServer = DNSServers.Count > 0 ? DNSServers[0].ToString() : string.Empty;
                        ConfigSecondaryDNSServer = DNSServers.Count > 1 ? DNSServers[1].ToString() : string.Empty;
                    }

                    CanConfigure = value.IsOperational;
                }

                _selectedNetworkInterface = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Details
        private string _detailsName;
        public string DetailsName
        {
            get { return _detailsName; }
            set
            {
                if (value == _detailsName)
                    return;

                _detailsName = value;
                OnPropertyChanged();
            }
        }

        private string _detailsDescription;
        public string DetailsDescription
        {
            get { return _detailsDescription; }
            set
            {
                if (value == _detailsDescription)
                    return;

                _detailsDescription = value;
                OnPropertyChanged();
            }
        }

        private string _detailsType;
        public string DetailsType
        {
            get { return _detailsType; }
            set
            {
                if (value == _detailsType)
                    return;

                _detailsType = value;
                OnPropertyChanged();
            }
        }

        private PhysicalAddress _detailsPhysicalAddress;
        public PhysicalAddress DetailsPhysicalAddress
        {
            get { return _detailsPhysicalAddress; }
            set
            {
                if (value == _detailsPhysicalAddress)
                    return;

                _detailsPhysicalAddress = value;
                OnPropertyChanged();
            }
        }

        private OperationalStatus _detailsStatus;
        public OperationalStatus DetailsStatus
        {
            get { return _detailsStatus; }
            set
            {
                if (value == _detailsStatus)
                    return;

                _detailsStatus = value;
                OnPropertyChanged();
            }
        }

        private long detailsSpeed;
        public long DetailsSpeed
        {
            get { return detailsSpeed; }
            set
            {
                if (value == detailsSpeed)
                    return;

                detailsSpeed = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv4Address;
        public IPAddress[] DetailsIPv4Address
        {
            get { return _detailsIPv4Address; }
            set
            {
                if (value == _detailsIPv4Address)
                    return;

                _detailsIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsSubnetmask;
        public IPAddress[] DetailsSubnetmask
        {
            get { return _detailsSubnetmask; }
            set
            {
                if (value == _detailsSubnetmask)
                    return;

                _detailsSubnetmask = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsGateway;
        public IPAddress[] DetailsIPv4Gateway
        {
            get { return _detailsGateway; }
            set
            {
                if (value == _detailsGateway)
                    return;

                _detailsGateway = value;
                OnPropertyChanged();
            }
        }

        private bool _detailsIPv4DhcpEnabled;
        public bool DetailsIPv4DhcpEnabled
        {
            get { return _detailsIPv4DhcpEnabled; }
            set
            {
                if (value == _detailsIPv4DhcpEnabled)
                    return;

                _detailsIPv4DhcpEnabled = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv4DhcpServer;
        public IPAddress[] DetailsIPv4DhcpServer
        {
            get { return _detailsIPv4DhcpServer; }
            set
            {
                if (value == _detailsIPv4DhcpServer)
                    return;

                _detailsIPv4DhcpServer = value;
                OnPropertyChanged();
            }
        }

        private DateTime _detailsDhcpLeaseExpires;
        public DateTime DetailsDhcpLeaseExpires
        {
            get { return _detailsDhcpLeaseExpires; }
            set
            {
                if (value == _detailsDhcpLeaseExpires)
                    return;

                _detailsDhcpLeaseExpires = value;
                OnPropertyChanged();
            }
        }

        private DateTime _detailsDhcpLeaseObtained;
        public DateTime DetailsDhcpLeaseObtained
        {
            get { return _detailsDhcpLeaseObtained; }
            set
            {
                if (value == _detailsDhcpLeaseObtained)
                    return;

                _detailsDhcpLeaseObtained = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6AddressLinkLocal;
        public IPAddress[] DetailsIPv6AddressLinkLocal
        {
            get { return _detailsIPv6AddressLinkLocal; }
            set
            {
                if (value == _detailsIPv6AddressLinkLocal)
                    return;


                _detailsIPv6AddressLinkLocal = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6Address;
        public IPAddress[] DetailsIPv6Address
        {
            get { return _detailsIPv6Address; }
            set
            {
                if (value == _detailsIPv6Address)
                    return;

                _detailsIPv6Address = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6Gateway;
        public IPAddress[] DetailsIPv6Gateway
        {
            get { return _detailsIPv6Gateway; }
            set
            {
                if (value == _detailsIPv6Gateway)
                    return;

                _detailsIPv6Gateway = value;
                OnPropertyChanged();
            }
        }

        private bool _detailsDNSAutoconfigurationEnabled;
        public bool DetailsDNSAutoconfigurationEnabled
        {
            get { return _detailsDNSAutoconfigurationEnabled; }
            set
            {
                if (value == _detailsDNSAutoconfigurationEnabled)
                    return;

                _detailsDNSAutoconfigurationEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _detailsDNSSuffix;
        public string DetailsDNSSuffix
        {
            get { return _detailsDNSSuffix; }
            set
            {
                if (value == _detailsDNSSuffix)
                    return;

                _detailsDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsDNSServer;
        public IPAddress[] DetailsDNSServer
        {
            get { return _detailsDNSServer; }
            set
            {
                if (value == _detailsDNSServer)
                    return;

                _detailsDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Config
        private bool _configEnableDynamicIPAddress = true;
        public bool ConfigEnableDynamicIPAddress
        {
            get { return _configEnableDynamicIPAddress; }
            set
            {
                if (value == _configEnableDynamicIPAddress)
                    return;

                _configEnableDynamicIPAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableStaticIPAddress;
        public bool ConfigEnableStaticIPAddress
        {
            get { return _configEnableStaticIPAddress; }
            set
            {
                if (value == _configEnableStaticIPAddress)
                    return;

                ConfigEnableStaticDNS = true;

                _configEnableStaticIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _configIPAddress;
        public string ConfigIPAddress
        {
            get { return _configIPAddress; }
            set
            {
                if (value == _configIPAddress)
                    return;

                _configIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _configSubnetmaskOrCidr;
        public string ConfigSubnetmaskOrCidr
        {
            get { return _configSubnetmaskOrCidr; }
            set
            {
                if (value == _configSubnetmaskOrCidr)
                    return;

                _configSubnetmaskOrCidr = value;
                OnPropertyChanged();
            }
        }

        private string _configGateway;
        public string ConfigGateway
        {
            get { return _configGateway; }
            set
            {
                if (value == _configGateway)
                    return;

                _configGateway = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableDynamicDNS = true;
        public bool ConfigEnableDynamicDNS
        {
            get { return _configEnableDynamicDNS; }
            set
            {
                if (value == _configEnableDynamicDNS)
                    return;

                _configEnableDynamicDNS = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableStaticDNS;
        public bool ConfigEnableStaticDNS
        {
            get { return _configEnableStaticDNS; }
            set
            {
                if (value == _configEnableStaticDNS)
                    return;

                _configEnableStaticDNS = value;
                OnPropertyChanged();
            }
        }

        private string _configPrimaryDNSServer;
        public string ConfigPrimaryDNSServer
        {
            get { return _configPrimaryDNSServer; }
            set
            {
                if (value == _configPrimaryDNSServer)
                    return;

                _configPrimaryDNSServer = value;
                OnPropertyChanged();
            }
        }

        private string _configSecondaryDNSServer;
        public string ConfigSecondaryDNSServer
        {
            get { return _configSecondaryDNSServer; }
            set
            {
                if (value == _configSecondaryDNSServer)
                    return;

                _configSecondaryDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Profiles
        ICollectionView _profiles;
        public ICollectionView Profiles
        {
            get { return _profiles; }
        }

        private ProfileInfo _selectedProfile = new ProfileInfo();
        public ProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null)
                {
                    ConfigEnableDynamicIPAddress = !value.NetworkInterface_EnableStaticIPAddress;
                    ConfigEnableStaticIPAddress = value.NetworkInterface_EnableStaticIPAddress;
                    ConfigIPAddress = value.NetworkInterface_IPAddress;
                    ConfigGateway = value.NetworkInterface_Gateway;
                    ConfigSubnetmaskOrCidr = value.NetworkInterface_SubnetmaskOrCidr;
                    ConfigEnableDynamicDNS = !value.NetworkInterface_EnableStaticDNS;
                    ConfigEnableStaticDNS = value.NetworkInterface_EnableStaticDNS;
                    ConfigPrimaryDNSServer = value.NetworkInterface_PrimaryDNSServer;
                    ConfigSecondaryDNSServer = value.NetworkInterface_SecondaryDNSServer;
                }

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                Profiles.Refresh();

                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.NetworkInterface_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get { return _profileWidth; }
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && value.Value != 40) // Do not save the size when collapsed
                    SettingsManager.Current.NetworkInterface_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion               
        #endregion

        #region Constructor, LoadSettings, OnShutdown
        public NetworkInterfaceViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Load network interfaces
            LoadNetworkInterfaces();

            _profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            _profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            _profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            _profiles.Filter = o =>
            {
                ProfileInfo info = o as ProfileInfo;

                if (string.IsNullOrEmpty(Search))
                    return info.NetworkInterface_Enabled;

                string search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(tagIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(info.Tags))
                        return false;
                    else
                        return (info.NetworkInterface_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(tagIdentifier.Length, search.Length - tagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase)));
                }
                else // Search by: Name
                {
                    return (info.NetworkInterface_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.IPScanner_IPRange.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1));
                }
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.NetworkInterface_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            LoadSettings();

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
            
            _isLoading = false;
        }

        private async void LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaceInfo info = NetworkInterfaces.Where(s => s.Id == SettingsManager.Current.NetworkInterface_SelectedInterfaceId).FirstOrDefault();

                if (info != null)
                    SelectedNetworkInterface = info;
                else
                    SelectedNetworkInterface = NetworkInterfaces[0];
            }

            IsNetworkInterfaceLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.NetworkInterface_ExpandProfileView;

            if (ExpandProfileView)
                ProfileWidth = new GridLength(SettingsManager.Current.NetworkInterface_ProfileWidth);
            else
                ProfileWidth = new GridLength(40);

            _tempProfileWidth = SettingsManager.Current.NetworkInterface_ProfileWidth;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadNetworkInterfacesCommand
        {
            get { return new RelayCommand(p => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute); }
        }

        private bool ReloadNetworkInterfaces_CanExecute(object obj)
        {
            return !IsNetworkInterfaceLoading;
        }

        private async void ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            string id = string.Empty;

            if (SelectedNetworkInterface != null)
                id = SelectedNetworkInterface.Id;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Change interface...
            SelectedNetworkInterface = string.IsNullOrEmpty(id) ? NetworkInterfaces.FirstOrDefault() : NetworkInterfaces.Where(x => x.Id == id).FirstOrDefault();

            IsNetworkInterfaceLoading = false;
        }

        public ICommand OpenNetworkConnectionsCommand
        {
            get { return new RelayCommand(p => OpenNetworkConnectionsAction()); }
        }

        public async void OpenNetworkConnectionsAction()
        {
            try
            {
                Process.Start("NCPA.cpl");
            }
            catch (Exception ex)
            {
                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Error"), ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ApplyNetworkInterfaceConfigCommand
        {
            get { return new RelayCommand(p => ApplyNetworkInterfaceConfigAction()); }
        }

        public void ApplyNetworkInterfaceConfigAction()
        {
            ApplyNetworkInterfaceConfig();
        }

        public ICommand ApplyProfileCommand
        {
            get { return new RelayCommand(p => ApplyProfileAction()); }
        }

        private void ApplyProfileAction()
        {
            ApplyProfile();
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_CopyProfile")
            };

            ProfileViewModel profileViewModel = new ProfileViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteProfile")
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, LocalizationManager.GetStringByKey("String_DeleteProfileMessage"));

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditGroupCommand
        {
            get { return new RelayCommand(p => EditGroupAction(p)); }
        }

        private async void EditGroupAction(object group)
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditGroup")
            };

            GroupViewModel editGroupViewModel = new GroupViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, group.ToString());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand FlushDNSCacheCommand
        {
            get { return new RelayCommand(p => FlushDNSCacheAction()); }
        }

        private void FlushDNSCacheAction()
        {
            FlushDNSCache();
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods
        public async void ApplyNetworkInterfaceConfig()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

            string subnetmask = ConfigSubnetmaskOrCidr;

            // CIDR to subnetmask
            if (ConfigEnableStaticIPAddress && subnetmask.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

            // If primary and secondary DNS are empty --> autoconfiguration
            if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) && string.IsNullOrEmpty(ConfigSecondaryDNSServer))
                ConfigEnableDynamicDNS = true;

            // When primary DNS is empty, swap it with secondary (if not empty)
            if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) && !string.IsNullOrEmpty(ConfigSecondaryDNSServer))
            {
                ConfigPrimaryDNSServer = ConfigSecondaryDNSServer;
                ConfigSecondaryDNSServer = string.Empty;
            }

            NetworkInterfaceConfig config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                EnableStaticIPAddress = ConfigEnableStaticIPAddress,
                IPAddress = ConfigIPAddress,
                Subnetmask = subnetmask,
                Gateway = ConfigGateway,
                EnableStaticDNS = ConfigEnableStaticDNS,
                PrimaryDNSServer = ConfigPrimaryDNSServer,
                SecondaryDNSServer = ConfigSecondaryDNSServer
            };

            try
            {
                Models.Network.NetworkInterface networkInterface = new Models.Network.NetworkInterface();

                networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

                await networkInterface.ConfigureNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async void ApplyProfile()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

            string subnetmask = SelectedProfile.NetworkInterface_SubnetmaskOrCidr;

            // CIDR to subnetmask
            if (SelectedProfile.NetworkInterface_EnableStaticIPAddress && subnetmask.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

            bool enableStaticDNS = SelectedProfile.NetworkInterface_EnableStaticDNS;

            string primaryDNSServer = SelectedProfile.NetworkInterface_PrimaryDNSServer;
            string secondaryDNSServer = SelectedProfile.NetworkInterface_SecondaryDNSServer;

            // If primary and secondary DNS are empty --> autoconfiguration
            if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && string.IsNullOrEmpty(secondaryDNSServer))
                enableStaticDNS = false;

            // When primary DNS is empty, swap it with secondary (if not empty)
            if (SelectedProfile.NetworkInterface_EnableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && !string.IsNullOrEmpty(secondaryDNSServer))
            {
                primaryDNSServer = secondaryDNSServer;
                secondaryDNSServer = string.Empty;
            }

            NetworkInterfaceConfig config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                EnableStaticIPAddress = SelectedProfile.NetworkInterface_EnableStaticIPAddress,
                IPAddress = SelectedProfile.NetworkInterface_IPAddress,
                Subnetmask = subnetmask,
                Gateway = SelectedProfile.NetworkInterface_Gateway,
                EnableStaticDNS = enableStaticDNS,
                PrimaryDNSServer = primaryDNSServer,
                SecondaryDNSServer = secondaryDNSServer
            };

            try
            {
                Models.Network.NetworkInterface networkInterface = new Models.Network.NetworkInterface();

                networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

                await networkInterface.ConfigureNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public void FlushDNSCache()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

            Models.Network.NetworkInterface.FlushDnsResolverCache();

            IsConfigurationRunning = false;
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                if (ProfileWidth.Value == 40)
                    ExpandProfileView = false;
                else
                    ExpandProfileView = true;
            }
            else
            {
                if (ExpandProfileView)
                {
                    if (_tempProfileWidth == 40)
                        ProfileWidth = new GridLength(250);
                    else
                        ProfileWidth = new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(40);
                }
            }

            _canProfileWidthChange = true;
        }
        #endregion

        #region Events
        private void NetworkInterface_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = LocalizationManager.GetStringByKey("String_CanceledByUser");
            DisplayStatusMessage = true;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.Window_ShowCurrentApplicationTitle))
                OnPropertyChanged(nameof(ShowCurrentApplicationTitle));
        }
        #endregion
    }
}
