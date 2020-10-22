new Vue({
    'el': '#app',
    data: {
        currenttime: "",
        cixiData: []
    },
    methods: {
        getCurrentTime: function () {
            dt = new Date();
            var y = dt.getFullYear();
            var mt = dt.getMonth() + 1;
            var day = dt.getDate();
            var h = dt.getHours();//获取时
            var m = dt.getMinutes();//获取分
            var s = dt.getSeconds();//获取秒
            //document.getElementById("currenttime").innerHTML = y + "年" + mt + "月" + day + "日 " + h + "时" + m + "分" + s + "秒";
            var pm = "pm";
            if (parseInt(h) < 13) {
                pm = "am";
            }

            var mtext = m < 10 ? "0" + m.toString() : m.toString();

            document.getElementById("currenttime").innerHTML = `<strong>${h}:${mtext}</strong>&nbsp;&nbsp;${pm}&nbsp;&nbsp;${y}-${mt}-${day}`;
        },
        change: function () {

        },
        play: function () {
            setInterval(this.change, 2000);//每两秒执行一次插入删除操作
        },
        //科研磁贴
        getCixiChart: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetRareDiseaseOverView",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    that.cixiData = response.data;
                }
            });
        },
        //患者趋势
        getAgeDistributionChartData: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetDiseaseTimeLineDistribution",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    age_line_chart(response.data);
                }
            });
        },
        //患者年龄段占比
        getDiseasePatienAgeData: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetDiseasePatientAge",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    age_pie_chart(response.data);
                }
            });
        },
        //患者性别占比
        getGetDiseasePatienSexData: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetDiseasePatientGender",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    sex_pie_chart(response.data);
                }
            });
        },

        //病种数据量排名
        getDiseasePatientRankData: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetDiseasePatientRank",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    disese_number_rank_chart(response.data);
                }
            });
        },
        //患者地域分布
        getPatientDomainDistributeData: function () {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Cabin/GetPatientAreaDistribution",
                contentType: "application/json"
            }).done(function (response) {
                if (response.success) {
                    china_map_chart(response.data);
                }
            });
        }

      
    },
    mounted: function () {
        this.getCixiChart(); // 病种磁贴
    },
    created: function () {
        var t = null;
        this.$nextTick(() => {
            t = setInterval(this.getCurrentTime, 1000);
            this.getAgeDistributionChartData();//患者趋势
            this.getDiseasePatienAgeData();//患者年龄段占比
            this.getGetDiseasePatienSexData();// 患者性别占比
            this.getPatientDomainDistributeData(); //患者地域分布
            this.getDiseasePatientRankData();//罕见病排名
        });
        this.play();
    }
});


var titleStyle = {
    color: '#fff',
    fontSize: 12,
    fontFamily: 'Microsoft YaHei',
    fontWeight: 400
};

function age_line_chart(data) {
    var option = {
        title: {
            text: '患者趋势',
            textStyle: titleStyle
        },
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: data.legendData,
            textStyle: titleStyle,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            align: 'left',
            right: 10
        },
        grid: {
            top: 70,
            left: '1%',
            right: '3%',
            bottom: '3%',
            containLabel: true
        },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            //data: ['2012,', '2013', '2014', '2015', '2016', '2017', '2018', '2019', '2020']
            data: data.xAxisData,
            axisLabel: {
                color: '#fff',
                fontSize: 12,
                fontFamily: 'Microsoft YaHei'
            },
            splitLine: {
                show: true,
                lineStyle: {
                    color: ['#616881'],
                    width: 1,
                    opacity: 0.2,
                    type: 'solid'
                }
            },
            axisLine: {       //y轴
                lineStyle: {
                    color: ['#616881'],
                    width: 1,
                    opacity: 0.2,
                    type: 'solid'
                }
            },
        },
        yAxis: {
            type: 'value',
            axisLabel: {
                color: '#fff',
                fontSize: 12,
                fontFamily: 'Microsoft YaHei'
            },
            splitLine: {
                show: true,
                lineStyle: {
                    color: ['#616881'],
                    width: 1,
                    opacity: 0.2,
                    type: 'solid'
                }
            },
            axisLine: {       //y轴
                lineStyle: {
                    color: ['#616881'],
                    width: 1,
                    opacity: 0.2,
                    type: 'solid'
                }
            }
        },
        series: [
            {
                name: '男',
                type: 'line',
                //stack: '数量',
                //data: [120, 132, 101, 134, 90, 230, 210, 132,120]
                data: data.lineData['男']
            },
            {
                name: '女',
                type: 'line',
                //stack: '数量',
                //data: [220, 182, 191, 234, 290, 330, 310,101, 134]
                data: data.lineData['女']
            }
        ],
        //legend 颜色的调色版
        color: ['#1890FF', '#E66C55']
    };

    var dom = document.getElementById("age_line_chart");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function age_pie_chart(data) {

    //data = {
    //    legendData: ['65-69岁', '70-74岁', '75-79岁', '80-85岁', '>85岁'],
    //    seriesData: [
    //        { value: 310, name: '65-69岁' },
    //        { value: 234, name: '70-74岁' },
    //        { value: 135, name: '75-79岁' },
    //        { value: 1548, name: '80-85岁' },
    //        { value: 1548, name: '>85岁' }
    //    ]
    //};

    var option = {
        title: {
            text: '患者年龄段占比',
            textStyle: titleStyle,
        },
        tooltip: {
            trigger: 'item',
            formatter: '{a} <br/>{b}: {c} ({d}%)'
        },
        grid: {
            left: '3%', //相当于距离左边效果:padding-left
            bottom: '3%',
            containLabel: true
        },
        legend: {
            type: 'plain',
            orient: 'vertical',
            right: 0,

            top: 30,
            bottom: 30,
            data: data.legendData,
            textStyle: titleStyle,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            formatter: function (name) {
                var data = option.series[0].data;
                var total = 0;
                var tarValue;
                for (var i = 0, l = data.length; i < l; i++) {
                    total += data[i].value;
                    if (data[i].name == name) {
                        tarValue = data[i].value;
                    }
                }
                var p = Math.round((tarValue / total) * 100);
                return name + " " + " " + p + "%";
            },
            //selected: data.selected
        },
        series: [
            {
                name: '患者年龄段占比',
                type: 'pie',
                radius: ['80%', '35%'],
                center: ['28%', '55%'],//调整图左右的位置
                avoidLabelOverlap: false,
                label: {
                    show: false,
                    position: 'center'
                },
                emphasis: {
                    label: {
                        show: true,
                        fontSize: '30',
                        fontWeight: 'bold'
                    }
                },
                labelLine: {
                    show: false
                },
                itemStyle: {
                    borderWidth: 6, //设置border的宽度有多大
                    borderColor: '#0D1D4C',
                },
                data: data.seriesData
            }
        ],
        color: ['#1890FF', '#00BCD5', '#7456F6', '#F7BF46', '#51BC5E']
    };
    var dom = document.getElementById("age_pie_chart");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function sex_pie_chart(data) {
    //data = {
    //    legendData: ['男', '女'],
    //    seriesData: [
    //        { value: 335, name: '男' },
    //        { value: 310, name: '女' }
    //    ]
    //};
    var option = {
        title: {
            text: '患者性别占比',
            textStyle: titleStyle,
        },
        tooltip: {
            trigger: 'item',
            formatter: '{a} <br/>{b}: {c} ({d}%)'
        },
        legend: {
            type: 'scroll',
            orient: 'vertical',
            right: 0,
            top: 50,
            bottom: 30,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            data: data.legendData,
            textStyle: titleStyle,
            formatter: function (name) {
                var data = option.series[0].data;
                var total = 0;
                var tarValue;
                for (var i = 0, l = data.length; i < l; i++) {
                    total += data[i].value;
                    if (data[i].name == name) {
                        tarValue = data[i].value;
                    }
                }
                var p = Math.round((tarValue / total) * 100);
                return name + " " + " " + p + "%";
            },
            //selected: data.selected
        },
        series: [
            {
                name: '患者性别占比',
                type: 'pie',
                radius: ['80%', '35%'],
                center: ['35%', '55%'],//调整图左右的位置
                avoidLabelOverlap: false,
                label: {
                    show: false,
                    position: 'center'
                },
                emphasis: {
                    label: {
                        show: true,
                        fontSize: '30',
                        fontWeight: 'bold'
                    }
                },
                labelLine: {
                    show: false
                },
                itemStyle: {
                    borderWidth: 6, //设置border的宽度有多大
                    borderColor: '#0D1D4C',
                },
                //data: [
                //    { value: 335, name: '直接访问' },
                //    { value: 310, name: '邮件营销' },
                //    { value: 234, name: '联盟广告' },
                //    { value: 135, name: '视频广告' },
                //    { value: 1548, name: '搜索引擎' }
                //]
                data: data.seriesData
            }
        ],
        color: ['#E66C55', '#1890FF']
    };
    var dom = document.getElementById("sex_pie_chart");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function disese_number_rank_chart(data) {
    //var data = {
    //    axisData: ['支气管或肺恶性肿瘤', '甲状腺恶性肿瘤', '垂体功能减退症', '多发性结肠息肉',
    //        '非霍奇金淋巴瘤', '多发性骨髓瘤', '老年性白内障', '老年性内风湿', '冠心病', '高血压'],
    //    seriesData: [981, 837, 744, 623, 555, 431, 326, 266, 201, 168]
    //};

    var option = {
        color: ['#7456F6'],
        grid: {
            top: 40,
            left: '1%',
            right: '5%',
            bottom: '5%',
            containLabel: true
        },
        title: {
            text: '罕见病数据量排名Top10',
            top:10,
            textStyle: titleStyle,
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
            }
        },
        legend: {
            data: ['数据量'],
            orient: 'vertical',
            align: 'left',
            top: 10,
            right: 1,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            textStyle: {
                color: '#fff',
                fontSize: 12,
                fontFamily: 'Microsoft YaHei'
            }
        },
        grid: {
            left: '3%',
            right: '8%',
            bottom: '3%',
            containLabel: true
        },
        xAxis: {
            type: 'value',
            show: false,
            splitLine: {
                show: false
            },
            axisLine: {       //y轴
                "show": false
            },
            axisTick: {
                show: false
            }
        },
        yAxis: {
            type: 'category',
            inverse: true,
            axisLabel: {
                margin: 20,
                textStyle: {
                    color: '#fff',
                    fontSize: 12,
                    fontFamily: 'Microsoft YaHei'
                }
            },
            data: data.axisData,
            splitLine: {
                show: false
            },
            axisLine: {       //y轴
                "show": false
            },
            axisTick: {
                show: false
            }
        },
        series: [
            {
                name: '数据量',
                type: 'bar',
                barCategoryGap: '35%',
                stack: '总量',
                label: {
                    show: true,
                    position: 'right',
                    color: '#ffffff',
                },
                data: data.seriesData,
                itemStyle: {
                    normal: {
                        //前4个参数用于配置渐变色的起止位置, 这4个参数依次对应右/下/左/上四个方位. 而0 0 0 1则代表渐变色从正上方开始.
                        color: new echarts.graphic.LinearGradient(0, 0, 1, 0, [{
                            offset: 0,
                            color: "#F7BF46" // 0% 处的颜色
                        }, {
                            offset: 1,
                            color: "#FFF0D1" // 100% 处的颜色
                        }], false)
                    }
                }
            }
        ]
    };

    var dom = document.getElementById("disease_number_rank");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}

function china_map_chart(data) {
    //data = [{ "name": "陕西", "value": 14237.0 }, { "name": "辽宁", "value": 4786.0 }, { "name": "山东", "value": 19678.0 }, { "name": "广东", "value": 23167.0 }, { "name": "四川", "value": 52389.0 }, { "name": "甘肃", "value": 4266.0 }, { "name": "广西", "value": 7843.0 }, { "name": "黑龙江", "value": 3423.0 }, { "name": "湖南", "value": 5456.0 }, { "name": "青海", "value": 1099.0 }, { "name": "内蒙古", "value": 2765.0 }, { "name": "河南", "value": 14906.0 }, { "name": "西藏", "value": 1377.0 }, { "name": "重庆", "value": 14678.0 }, { "name": "云南", "value": 9876.0 }, { "name": "宁夏", "value": 1122.0 }, { "name": "新疆", "value": 4578.0 }, { "name": "贵州", "value": 7659.0 }, { "name": "湖北", "value": 18734.0 }, { "name": "江西", "value": 5332.0 }, { "name": "福建", "value": 8652.0 }, { "name": "浙江", "value": 10874.0 }, { "name": "江苏", "value": 14532.0 }, { "name": "安徽", "value": 10995.0 }, { "name": "山西", "value": 8754.0 }, { "name": "河北", "value": 7542.0 }, { "name": "北京", "value": 10278.0 }, { "name": "天津", "value": 3652.0 }, { "name": "上海", "value": 11334.0 }, { "name": "吉林", "value": 7742.0 }, { "name": "黑龙江", "value": 4561.0 }, { "name": "海南", "value": 2347.0 }];
    var res = [];
    for (var i = 0; i < data.length; i++) {
        res.push({
            name: data[i].name,
            value: data[i].value / 1000
        });
    }
   // data = res;
    var option = {
        title: {
            text: ''
        },
        // 鼠标移到图里面的浮动提示框
        tooltip: {
            trigger: 'item',
            formatter: function (data) {
                if (data.data.name === "四川") {
                    return data.data.name + "：25651";
                }
                return data.data.name + "：" + data.data.value;
            }
        },
        dataRange: {
            show: true,
            min: 0,
            textStyle: {
                color: '#fff',
                fontSize: 12,
                fontFamily: 'Microsoft YaHei'
            },
            max: 250,
            range: [0, 199],
            text: ['', ''],
            realtime: false,
            calculable: false,
            startAngle: 0,
            color: [],
            orient: 'vertical',
            itemWidth: '15',
            itemHeight: '120',
            left: 20,
            bottom: 20,
            precision: 0  // 数据展示的小数精度
        },
        geo: { // 这个是重点配置区
            map: 'china', // 表示中国地图
            roam: true,
            z: '2',
            scaleLimit: {
                min: 1.8,
                max: 1.8
            },
            label: {
                normal: {
                    show: false, // 是否显示对应地名
                    textStyle: {
                        color: '#c3e5dc'
                    }
                }
            },
            itemStyle: {
                normal: {
                    borderColor: 'rgba(0, 0, 0, 0.2)'
                    // areaColor: '#ededed' // 地图背景色
                },
                emphasis: {
                    areaColor: null,
                    shadowOffsetX: 0,
                    shadowOffsetY: 0,
                    shadowBlur: 20,
                    borderWidth: 0,
                    shadowColor: 'rgba(0, 0, 0, 0.5)'
                }
            },
            right: '100',
            left: 'auto',
            bottom: '0'
        },
        series: [
            {
                type: 'scatter',
                coordinateSystem: 'geo' // 对应上方配置
            },
            {
                name: '', // 浮动框的标题
                type: 'map',
                mapType: 'china',
                geoIndex: 0,
                data: []
            }
        ]
    };
    option.series[1].data = data;
    option.dataRange.color = ['#990000', '#FF6340', '#FFFFFF'];
    // obj.geo.itemStyle.normal.areaColor = '#eedcb9'
    var maxNum = 0;
    var minNum = 0;
    if (data && data.length > 0) {
        var values = data.map(f => f.value);
        maxNum = Math.max.apply(null, values);
        minNum = Math.min.apply(null, values);
    }
    option.dataRange.min = minNum;
    option.dataRange.max = maxNum;
    option.dataRange.range = [minNum, maxNum];
    option.dataRange.text = ['高', '低'];
    option.right = 'auto';

    var dom = document.getElementById("china_map_chart");
    var myChart = echarts.init(dom);
    if (option && typeof option === "object") {
        myChart.setOption(option, true);
    }

    window.addEventListener("resize", function () {
        myChart.resize();
    });
}