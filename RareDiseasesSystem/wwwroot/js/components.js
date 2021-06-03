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
                search: "帕金森",
                deseaseSearchedList: [],
                total: 0,
                pageIndex: 1,
                loading: false,
                iframeSrc: " ",
                pageSize:5
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
                param.pageSize = 5;
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
                    }
                });
            },
            open121HpoDetail: function (subgroup) {
                document.getElementById("iframeDiv").innerHTML = "";
                var iframeSrc = "../html/121.html" + subgroup.anchor;


                //在document中创建iframe
                var iframe = document.createElement("iframe");

                //设置iframe的样式
                iframe.style.width = "100%";
                iframe.style.height = "calc(100vh - 100px)";
                iframe.style.margin = "0";
                iframe.style.padding = "0";
                iframe.style.border = "1 solid  rgb(243,247,254)";

                //绑定iframe的onload事件,处理事件的兼容问题
                if (
                    onload &&
                    Object.prototype.toString.call(onload) === "[object Function]"
                ) {
                    if (iframe.attachEvent) {
                        iframe.attachEvent("onload", onload);
                    } else if (iframe.addEventListener) {
                        iframe.addEventListener("load", onload);

                    } else {
                        iframe.onload = onload;
                    }
                }

                iframe.src = iframeSrc;
                //把iframe载入到dom以下
                document.getElementById("iframeDiv").appendChild(iframe);
                return iframe;
            }
        }
    });

})();

(function () {
    Vue.component("v-disease-cdss", {
        data: function () {
            return {
                importEMRDlg: false,
                number: "", //number 可以是empiid 或者身份证号

                numberType: "empi",
                numberTypeOptions: [
                    {
                        value: 'empi',
                        label: 'EMPI主索引'
                    },
                    {
                        value: 'card',
                        label: '身份证号'
                    }
                ],
                patientOverview: [],
                patientVisitList: [],
                patientVisitListCopy: [],
                patientEMRDetail: "",
                patientVisitListIndex: 1,
                patientVisitListTotal: 0,
                multiplePatientVisitSelection: [],

                patientHPOList: [],
                searchHPOText: "",
                searchHPODlg: false,
                searchedHPOList: [],
                searchedHPOListCopy: [],
                searchedHPOListTotal: 0,
                searchedHPOListIndex: 1,
                multipleSearchedHPOSelection: [],

                //normalDiseaseList: [],
                rareDiseaseList: [],

                nlpEngine: "String_Search",
                nlpEngineOptions: [
                    {
                        value: 'String_Search',
                        label: '术语集搜索(约8万条)'
                    },
                    {
                        value: 'Spacy',
                        label: '深度学习实体识别(约72万条)'
                    }
                ],
                certainOptions: [
                    {
                        value: '阴性',
                        label: '阴性'
                    },{
                    value: '阳性',
                    label: '阳性'
                }],
                rareAnalyzeEngine: "Jaccard",
                rareAnalyzeEngineOptions: [
                    {
                        value: 'Jaccard',
                        label: 'Jaccard'
                    },
                    {
                        value: 'Tanimoto',
                        label: 'Tanimoto'
                    },
                    {
                        value: 'Overlap',
                        label: 'Overlap'
                    },
                    {
                        value: 'Oss',
                        label: 'Oss'
                    },
                    {
                        value: 'Loglikelihood',
                        label: 'Log-likelihood & ratio'
                    }

                ],
                rareDataBaseEngine: "eRAM",
                rareDataBaseEngineOptions: [
                    {
                        value: 'eRAM',
                        label: 'eRAM'
                    },
                    {
                        value: 'OMIM',
                        label: 'OMIM'
                    },
                       //for nlp engine
                    {
                        value: 'ORPHA',
                        label: 'ORPHANET'
                    },
                    {
                        value: 'DECIPHER',
                        label: 'DECIPHER'
                    },
                       //for nlp engine
                    {
                        value: 'all',
                        label: '整合库'
                    }
                ],
                clickedHPOText: "" ,// 一个HPO 有多次文本匹配，记住上次点击的那一个
                clickedHPOIndex:0
                
            };
        },
        template: "#v-disease-cdss",    
        methods: {
            onPopImportEMRDlg: function () {
                this.importEMRDlg = true;
            },
            onSearchPatient: function () {
                this.patientVisitListIndex = 1;
                this.requestPatientData();
                this.patientEMRDetail = "";
                this.multiplePatientVisitSelection = [];
            },
            requestPatientData: function () {
                var para = {};
                para = {
                    number: this.number,
                    numberType: this.numberType

                };
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'lightgray'
                });
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
                            that.patientVisitPaging();
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            },
            patientVisitPaging: function () {
                this.patientVisitList = this.localPaging(this.patientVisitListIndex, this.patientVisitListCopy);
            },
            onPatientVisitSelectionChange: function (val) {
                this.multiplePatientVisitSelection = val;
            },
            //根据所选取的就诊记录，查询数据
            onImportPatientEMRText: function () {
                var visitIds = this.multiplePatientVisitSelection.map(function (item) {
                    return item.visitid;
                });

                if (visitIds.length === 0) {
                    alert("请选择相应的就诊记录");
                    return;
                }
                if (visitIds.length > 1) {
                    alert("由于电子病历太长，最多只能选一次就诊病历");
                    return;
                }
                this.patientEMRDetail = "";
                this.patientHPOList = [];
                var para = {};
                para = {
                    patientVisitIds: visitIds.toString()
                };
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                $.ajax({
                    url: "/Home/GetPatientEMRDetail",
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
                        loading.close();
                    }
                });
                this.importEMRDlg = false;
            },
            onClearPatientEMR: function () {
                this.patientEMRDetail = "";
                this.patientHPOList = [];
                this.rareDiseaseList = [];
                this.$refs.multipleTable.clearSelection();
            },
            //这个地方有两种情况
            //1. 用户直接传一段文本，和病人没有关联，这个时候直接分析文本
            //2. 如果就诊记录 不为空，说明是获取刚才查询病人的HPO 结果，这个情况根据就诊记录直接从数据库获取
            onGetPatientHPOResult: function () {
                if (this.patientEMRDetail === undefined || this.patientEMRDetail === "") {
                    alert('电子病历不能为空！');
                    return false;
                }
                this.patientHPOList = [];
                this.rareDiseaseList = [];
                var visitIds = this.multiplePatientVisitSelection.map(function (item) {
                    return item.visitid;
                });
                var para = {};

                para = {
                    patientEMRDetail: encodeURI(this.patientEMRDetail),
                    patientVisitIds: visitIds.toString(),
                    nlpEngine: this.nlpEngine
                };
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                $.ajax({
                    url: "/Home/GetPatientHPOResult",
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
                        loading.close();
                    }
                });
            },
            onDeletePatientHPOList: function (subgroup) {
                for (var i = this.patientHPOList.length - 1; i >= 0; i--) {
                    if (this.patientHPOList[i].name === subgroup.name && this.patientHPOList[i].hpoId === subgroup.hpoId) {
                        this.patientHPOList.splice(i, 1);
                    }
                }
            },
            onDeleteHPOTerm: function (subgroup) {
                var para = {};
                para = {
                    hpoId: subgroup.hpoId,
                    hpoTerm: subgroup.name
                };
                var that = this;
                $.ajax({
                    url: "/Home/DeleteHPOTerm",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        that.onDeletePatientHPOList(subgroup);
                    }
                });
            },
            onShowHpoMatchedText: function (subgroup) {
                //如果有多次匹配，那么循环显示
                var indexData;
                if (this.clickedHPOText === subgroup.hpoId) {
                    if (this.clickedHPOIndex < subgroup.indexList.length - 1) {
                        this.clickedHPOIndex++;
                    }
                    else {
                        this.clickedHPOIndex = 0;
                    }
                    indexData = subgroup.indexList[this.clickedHPOIndex];
                }
                else {
                    this.clickedHPOIndex = 0;
                    indexData = subgroup.indexList[0];
                    this.clickedHPOText = subgroup.hpoId;
                }
                var startIndex = indexData.startIndex;
                var endIndex = indexData.endIndex;

                var text = $('#txt_patientEMR').val();
                var textBeforePosition = text.substr(0, startIndex);
                $('#txt_patientEMR').blur();
                $('#txt_patientEMR').val(textBeforePosition);
                $('#txt_patientEMR').focus();
                $('#txt_patientEMR').val(text);
                $('#txt_patientEMR').selectRange(startIndex, endIndex);

            },
            onSearchHPODlg: function () {
                this.searchHPODlg = true;
            },
            //查询表征HPO 库，自定义添加HPO
            onSearchHPOList: function () {
                this.searchedHPOListIndex = 1;
                var para = {};
                para = {
                    searchHPOText: this.searchHPOText
                };
                this.loading = true;
                var that = this;
                $.ajax({
                    url: "/Home/SearchStandardHPOList",
                    type: "Get",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.searchedHPOListTotal = data.total;
                            that.searchedHPOListCopy = data.data;
                            that.HPOLocalPaging();
                        }
                        else {
                            console.log(data);
                        }
                        that.loading = false;
                    }
                });
            },
            HPOLocalPaging: function () {
                this.searchedHPOList = this.localPaging(this.searchedHPOListIndex, this.searchedHPOListCopy);
            },
            localPaging: function (current, list) {
                var size = 10;
                var tablePush = [];
                for (var i = 0; i < list.length; i++) {
                    if (size * (current - 1) <= i && i <= size * current - 1) {
                        tablePush.push(list[i]);
                    }
                };
                //subList.forEach((item, index) => {
                //    if (size * (current - 1) <= index && index <= size * current - 1) {
                //        tablePush.push(item);
                //    }
                //});
                return tablePush;
            },

            onSearchedHPOSelectionChange: function (val) {
                this.multipleSearchedHPOSelection = val;
            },
            onSearchedHPOSelectionIn: function () {
                var hpoIds = this.multipleSearchedHPOSelection.map(function (item) {
                    return item.hpoId;
                });

                if (hpoIds.length === 0) {
                    alert("请选择相应的HPO 表征");
                    return;
                }
                else {
                    var that = this;
                    this.multipleSearchedHPOSelection.map(function (item) {
                        var exist = false;
                        for (var i = 0; i < that.patientHPOList.length; i++) {
                            if (that.patientHPOList[i].hpoId === item.hpoId) {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist) {
                            that.patientHPOList.push(item);
                        }

                    });
                    this.searchHPODlg = false;

                }
            },
            onGetPatientRareDiseaseResult: function () {
                if (this.rareAnalyzeEngine === undefined || this.rareAnalyzeEngine === "") {
                    alert('请选择罕见病分析引擎！');
                    return false;
                }
                if (this.rareDataBaseEngine === undefined || this.rareDataBaseEngine === "") {
                    alert('请选择罕见病数据库！');
                    return false;
                }
                this.rareDiseaseList = [];
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                var para = {};
                var hpoStr = this.GetHpoStr();
                para = {
                    hpoStr: hpoStr,
                    rareAnalyzeEngine: this.rareAnalyzeEngine,
                    rareDataBaseEngine: this.rareDataBaseEngine
                };
                $.ajax({
                    url: "/RareDisease/GetPatientRareDiseaseResult",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            //that.normalDiseaseList = data.normalDiseaseList;
                            that.rareDiseaseList = data.rareDiseaseList;
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            },
            onRedirectToDiseaseCaculate: function () {
                sessionStorage["diseaseCaculate-hpoStr"] = this.GetHpoStr();
                window.location.href = "/home/DiseaseCaculate";
            },
            statusFormatter(row, column) {
                let status = row.isGenerated;
                if (status === 0) {
                    return '否';
                } else {
                    return '是';
                }
            },
            rowClass(row, index) {
                if (row.row.match === 1) {
                    return { "background-color": "springgreen" };
                }
            },
            GetHpoStr: function () {
                var hpoStr = "";
                for (var i = 0; i < this.patientHPOList.length; i++) {
                    if (this.patientHPOList[i].certain === "阳性" && hpoStr.indexOf(this.patientHPOList[i].hpoId) < 0) {
                        hpoStr += this.patientHPOList[i].hpoId + ",";
                    }
                }
                hpoStr = hpoStr.substring(0, hpoStr.length - 1);
                return hpoStr;
            },
            filterTag(value, row) {
                return row.certain === value;
            }
        },
        mounted: function () {
            sessionStorage["diseaseCaculate-hpoStr"] = "";
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
                loading: false,
                logDateRange:""
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
                param.logDateRange = this.logDateRange;
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
                    }
                });
            }
        },
        mounted: function () {
            this.onSearch();
        }
    });

})();


(function () {
    Vue.component("v-disease-caculate", {
        data: function () {
            return {
                HPOStr: "HP:0001345,HP:0001545,HP:0002043,HP:0000707,HP:0003268,HP:0000787,HP:0003828,HP:0003131",
                dataBaseEngine:"eRAM",
                dataBaseEngineOptions: [
                    {
                        value: 'eRAM',
                        label: 'eRAM'
                    },
                    {
                        value: 'OMIM',
                        label: 'OMIM'
                    },
                    //for nlp engine
                    {
                        value: 'ORPHA',
                        label: 'ORPHANET'
                    },
                    {
                        value: 'DECIPHER',
                        label: 'DECIPHER'
                    },
                    //for nlp engine
                    {
                        value: 'all',
                        label: '整合库'
                    }
                ],
                overviewList: [],
                diseaseList: [],
                diseaseCaluateBar:[]
            };
        },
        template: "#v-disease-caculate",
        methods: {
            onGetDiseaseCaculateResult: function () {
                if (this.HPOStr === undefined || this.HPOStr === "") {
                    alert('请输入HPO！');
                    return false;
                }
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                var that = this;
                var para = {};
                para = {
                    hpoStr: this.HPOStr,
                    rareAnalyzeEngine: "Jaccard,Tanimoto,Overlap",
                   // rareAnalyzeEngine: "Jaccard,Tanimoto,Overlap,Loglikelihood",
                    rareDataBaseEngine: this.dataBaseEngine
                };
                $.ajax({
                    url: "/RareDisease/GetDiseaseCaculateResult",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                            that.overviewList = data.overviewList;
                            that.diseaseList = data.diseaseList;
                            that.diseaseCaluateBar = data.diseaseCaluateBar;
                            disease_hpo_bar_chart(data.diseaseCaluateBar);
                            disease_hpo_distribution_chart(data.diseaseCaculateDistribution);
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            }
        },
        mounted: function () {
            if (sessionStorage["diseaseCaculate-hpoStr"] !== "") {
                this.HPOStr = sessionStorage["diseaseCaculate-hpoStr"];
            }
            document.getElementsByClassName("login-footer")[0].style.position = 'relative';
        
        }
    });

})();


function disease_hpo_distribution_chart(disease) {
    option = {
        legend: {
            data: ['HPO'],
            left: 'right'
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            containLabel: true
        },
        xAxis: {
            type: 'category',
            data: disease.xAxis,
            boundaryGap: false,
            splitLine: {
                show: true,
                lineStyle: {
                    color: '#999',
                    type: 'dashed'
                }
            },
            axisLabel: {
                interval: 0,
                rotate: 40
            } 
        },
        yAxis: {
            type: 'category',
            data: disease.yAxis,
            inverse: true,
            boundaryGap: false,
            splitLine: {
                show: true,
                lineStyle: {
                    color: '#999',
                    type: 'dashed'
                }
            },
            axisLabel: {
                interval: 0,
                formatter: function (value) {
                    if (value.length > 50) {
                        value = value.slice(0, 100) + '...';
                    }
                    return value;
                },
                tooltip: {
                    show: true
                }
            } 
            
        },
        series: [{
            name: 'HPO',
            type: 'scatter',
            symbolSize: function (val) {
                return 10;
            },
            color: function (val) {
                if (val.data[2] === 1) {
                    return "red"; 
                }
                else {
                    return "gray";
                }
               
            },
            data: disease.marks
        }]
    };

    var dom = document.getElementById("disease_hpo_distribution");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}


function disease_hpo_bar_chart(disease) {
    option = {
        tooltip: {
            trigger: 'axis',
            axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
            }
        },
        legend: {
            data: disease.SeriesData.map(function (item) { return item.name; })
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            containLabel: true
        },
        xAxis: {
            type: 'value'
        },
        yAxis: {
            type: 'category',
            data: disease.yAxis,
            inverse: true,
            axisLabel: {
                interval: 0,
                formatter: function (value) {
                    if (value.length > 50) {
                        value = value.slice(0, 100) + '...';
                    }
                    return value;
                },
                tooltip: {
                    show: true
                }
            }
        },
        series: [
            {
                name: disease.SeriesData[0].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideRight'
                },
                data: disease.SeriesData[0].value
            },
            {
                name: disease.SeriesData[1].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideLeft'
                },
                data: disease.SeriesData[1].value
            },
            {
                name: disease.SeriesData[2].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideLeft'
                },
                data: disease.SeriesData[2].value
            },
            {
                name: disease.SeriesData[3].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideLeft'
                },
                data: disease.SeriesData[3].value
            }
        ]
    };

    var dom = document.getElementById("disease_hpo_bar");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}


(function () {
    Vue.component("v-disease-summary", {
        data: function () {
            return {
                diseaseText: "21-羟化酶缺乏症",
                diseaseOptions: [],
                casesCount: 0,
                diseaseAllData: [],
                displayText: "hpoId",
                displayOptions: [
                    {
                        value: 'hpoId',
                        label: 'HPO ID'
                    },
                    {
                        value: 'Chinease',
                        label: '中文 HPO'
                    },
                    //for nlp engine
                    {
                        value: 'English',
                        label: '英文 HPO'
                    }
                ],
                HideHpoStr: "",
                HPOMinLimit:10
            };
        },
        template: "#v-disease-summary",
        methods: {
            onDiseaseSummaryResult: function () {
                const loading = this.$loading({
                    lock: true,
                    text: '拼命加载中...',
                    spinner: 'el-icon-loading',
                    background: 'rgba(0, 0, 0, 0.7)'
                });
                para = {
                    diseaseText: encodeURI(this.diseaseText),
                    hideHpoStr: encodeURI(this.HideHpoStr),
                    minCount: this.HPOMinLimit
                };
                var that = this;
                $.ajax({
                    url: "/RareDisease/GetDiseaseHPOSummaryBarAsync",
                    type: "POST",
                    data: para,
                    dataType: 'json',
                    success: function (data) {
                        if (data && data.success) {
                        
                            that.diseaseAllData = data.data;
                            that.casesCount = that.diseaseAllData.casesCount;
                            var dom = document.getElementById("disease_hpo_bar");
                            dom.innerHTML === "";
                            dom.removeAttribute("_echarts_instance_");
                            if (that.diseaseAllData.yAxis.length > 1000) {
                                dom.style.height = (that.diseaseAllData.yAxis.length * 15) + "px";
                            }
                            else {
                                dom.style.height = (that.diseaseAllData.yAxis.length * 25) + "px";
                            }
                            disease_hpo_bar_chart_summary(data.data, that.diseaseAllData.yAxis.map(function (item) { return item.hpoId; }));
                        }
                        else {
                            console.log(data);
                        }
                        loading.close();
                    }
                });
            },
            onSwtichSummaryResult: function () {
                if (this.displayText === "hpoId") {
                    disease_hpo_bar_chart_summary(this.diseaseAllData, this.diseaseAllData.yAxis.map(function (item) { return item.hpoId; }));
                }
                if (this.displayText === "Chinease") {
                    disease_hpo_bar_chart_summary(this.diseaseAllData, this.diseaseAllData.yAxis.map(function (item) { return item.name_cn; }));
                }
                if (this.displayText === "English") {
                    disease_hpo_bar_chart_summary(this.diseaseAllData, this.diseaseAllData.yAxis.map(function (item) { return item.name_en; }));
                }
            }
        },
        mounted: function () {
            document.getElementsByClassName("login-footer")[0].style.position = 'relative';
            const loading = this.$loading({
                lock: true,
                text: '拼命加载中...',
                spinner: 'el-icon-loading',
                background: 'rgba(0, 0, 0, 0.7)'
            });
            var that = this;
            $.ajax({
                url: "/RareDisease/GetDiseaseNameList",
                type: "GET",
                dataType: 'json',
                success: function (data) {
                    if (data && data.success) {
                        that.diseaseOptions = data.data;
                    }
                    else {
                        console.log(data);
                    }
                },
                complete: function () {
                    loading.close();
                }
            });
        }
    });

})();

function disease_hpo_bar_chart_summary(disease,yAxisData) {
    option = {
        tooltip: {
            trigger: 'axis',
            axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
            },
            formatter: function (params) {
                var output = "";
                for (i = 0; i < params.length; i++) {
                    if ((params[i].seriesName === "Coverd by eRAM" || params[i].seriesName === "Coverd by OMIM" || params[i].seriesName === "Coverd by Orphanet") && params[i].value < 0) {
                        output += params[i].marker + params[i].seriesName + ': ' + 1;
                    }
                    else {
                        output += params[i].marker + params[i].seriesName + ': ' + params[i].value; 
                    }
                   
                    if (i !== params.length - 1) { 
                        output += '<br/>';
                    }
                }
                return output;
           }
        },
        legend: {
            textStyle: { //图例文字的样式
                fontSize: 16,
                padding: [0, 20, 0, 0]
            },
            data: disease.SeriesData.map(function (item) { return item.name; })
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '3%',
            top:40,
            containLabel: true
        },
        xAxis: {
            type: 'value'
        },
        yAxis: {
            type: 'category',
            data: yAxisData, 
            inverse: true,
            axisLabel: {
                interval: 0,
                formatter: function (value) {
                    if (value.length > 100) {
                        value = value.slice(0, 100) + '...';
                    }
                    return value;
                },
                tooltip: {
                    show: true
                }
            }
        },
        series: [
            {
                name: disease.SeriesData[0].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideRight',
                    formatter: function (params) {
                        if (params.value === 0) {
                            return "";
                        }
                        else {
                            return params.value;
                        }
                    }
                },
                data: disease.SeriesData[0].value
            },
            {
                name: disease.SeriesData[1].name,
                type: 'bar',
                stack: '总量',
                label: {
                    show: true,
                    position: 'insideRight',
                    formatter: function (params) {
                        if (params.value === 0) {
                            return "";
                        }
                        else {
                            return params.value;
                        }
                    }
                },
                data: disease.SeriesData[1].value
            },
            {
                name: disease.SeriesData[2].name,
                type: 'bar',
                stack: '总量',
                data: disease.SeriesData[2].value
            },
            {
                name: disease.SeriesData[3].name,
                type: 'bar',
                stack: '总量',
                data: disease.SeriesData[3].value
            },
            {
                name: disease.SeriesData[4].name,
                type: 'bar',
                stack: '总量',
                itemStyle: {
                    normal: { color: '#72b201' }
                },
                data: disease.SeriesData[4].value
            }
        ]
    };

    var dom = document.getElementById("disease_hpo_bar");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }
    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

