(function () {
    Vue.component("v-user-menu", {
        template: "#v-user-menu",
        props: ['loginUser'],
        methods: {
            menuClick: function (command) {
                if (command === "logout") {
                    this.logout();
                }            
            },
            logout: function () {
                if (confirm("您确定要注销么？")) {
                    var returnUrl = window.location.href;
                    window.location.href = "/Login/Index?Logout=true&ReturnUrl=" + encodeURI(returnUrl);
                }
            }
        }
    });
})();

(function () {
    Vue.component("v-disease-search", {
        data: function () {
            return {
                search: "",
                deseaseSearchedList: [],
                total: 0,
                pageIndex: 1,
                loading: false
            };
        },
        template: "#v-disease-search",
        methods: {

            onSearch: function () {
                this.pageIndex = 1;
                this.searchDiseaseList();
            },
            searchDiseaseList: function () {
                var param = {};
                param.search = this.search;
                param.pageIndex = this.pageIndex;
                param.pageSize = 10;
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/RareDisease/SearchList",
                    type: "GET",
                    data: param,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.deseaseSearchedList = data.data;
                            that.total = data.total;
                        }
                        else {
                            console.log(data.msg);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false; }
                });
            }
        }
    });

})();

(function () {
    Vue.component("v-disease-cdss", {
        data: function () {
            return {
                importEMRDlg: false,
                patientCardNo: "",
                patientOverview: [],
                patientVisitList: [],
                patientVisitListCopy: [],
                patientEMRDetail: "",
                patientVisitListIndex: 1,
                patientVisitListTotal: 0,

                patientHPOList: [],
                searchHPOText: "",
                searchHPODlg: false,
                searchedHPOList: [],
                searchedHPOListCopy: [],
                searchedHPOListTotal: 0,
                searchedHPOListIndex: 1,
                multipleSearchedHPOSelection: [],

                normalDiseaseList: [],
                rareDiseaseList: []
            };
        },
        template: "#v-disease-cdss",    
        methods: {
            onPopImportEMRDlg: function () {
                this.importEMRDlg = true;
            },
            onSearchPatient: function () {
                this.patientVisitListIndex = 1;
                this.requestPatientEMR();
            },
            requestPatientEMR: function () {
                var para = {};
                para = {
                    patientCardNo: this.patientCardNo
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchPatientData",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientVisitListTotal = data.total;
                            that.patientVisitListCopy = data.patientVisitList; 
                            that.patientOverview = data.patientOverview;
                            that.patientLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false;}
                });
            },
            patientLocalPaging: function () {
                this.patientVisitList = this.localPaging(this.patientVisitListIndex, this.patientVisitListCopy);
            },

            onImportPatientEMRText: function () {
                var para = {};
                para = {
                    patientCardNo: this.patientCardNo
                };                
                var that = this;
                $.ajax({
                    url: "/Home/ConvertPatientEMRtoText",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientEMRDetail = data.data;
                        }
                        else {
                            console.log(data);
                        }
                    },
                    error: () => { that.loading = false; }
                });
                this.importEMRDlg = false;
            },
            onClearPatientEMR: function () {
                this.patientEMRDetail = "";
            },

            onAnalyzePatientEMR: function () {
                var para = {};
                para = {
                    patientEMRDetail: this.patientEMRDetail
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/AnalyzePatientEMRRetreiveHPO",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.patientHPOList = data.data;
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false; }
                });
            },
            onDeletePatientHPOList: function (subgroup) {
                for (var i = this.patientHPOList.length - 1; i >= 0; i--) {
                    if (this.patientHPOList[i].name === subgroup.name) {
                        this.patientHPOList.splice(i, 1);
                    }
                }
            },
            onShowHpoMatchedText: function (subgroup) {
                alert("1");
                var node = document.getElementById("txt_patientEMR");
                node.selectionStart = 2;
                node.selectionEnd = 7;
                node.focus();
            },
            onSearchHPODlg: function () {
                this.searchHPODlg = true;
            },
            onSearchHPOList: function () {
                this.searchedHPOListIndex = 1;
                var para = {};
                para = {
                    searchHPOText: this.searchHPOText
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchHPOList",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.searchedHPOListTotal = data.total;
                            that.searchedHPOListCopy = data.data;
                            that.hpoLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false; }
                });
            },
            hpoLocalPaging: function () {
                this.searchedHPOList = this.localPaging(this.searchedHPOListIndex, this.searchedHPOListCopy);
            },
            localPaging: function (current,list) {
                var size = 10;
                const subList = list;
                const tablePush = [];
                subList.forEach((item, index) => {
                    if (size * (current - 1) <= index && index <= size * current - 1) {
                        tablePush.push(item);
                    }
                });
                return tablePush;
            },

            onSearchedHPOSelectionChange: function (val) {
                this.multipleSearchedHPOSelection = val;
            },
            onSearchedHPOSelectionIn: function () {
                var names = this.multipleSearchedHPOSelection.map(function (item) {
                    return item.name;
                });

                if (names.length === 0) {
                    alert("请选择相应的HPO 表征");
                    return;
                }
                else {
                    var that = this;
                    this.multipleSearchedHPOSelection.map(function (item) {
                        that.patientHPOList.push(item);
                    });
                    this.searchHPODlg = false;
                        
                }
            },
            onSubmitHPODataForAnalyze: function () {
                var para = {};
                para = {
                    hpoList: this.patientHPOList
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SubmitHPODataForAnalyze",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.normalDiseaseList = data.normalDiseaseList;
                            that.rareDiseaseList = data.rareDiseaseList;
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false; }
                });
            }
        }
    });
})();



(function () {
    Vue.component("v-operation-log", {
        data: function () {
            return {
                logList: [],
                total: 0,
                pageIndex: 1,
                loading: false
            };
        },
        template: "#v-operation-log",
        methods: {

            onSearch: function () {
                this.pageIndex = 1;
                this.searchOperationLogList();
            },
            searchOperationLogList: function () {
                var param = {};
                param.pageIndex = this.pageIndex;
                param.pageSize = 10;
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Log/SearchLogList",
                    type: "GET",
                    data: param,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.logList = data.data;
                            that.total = data.total;
                        }
                        else {
                            console.log(data.msg);
                        }
                        that.loading = false;
                    },
                    error: () => { that.loading = false; }
                });
            }
        },
        mounted: function () {
            this.onSearch();
        }
    });

})();