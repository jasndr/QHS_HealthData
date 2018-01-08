libname trend 'C:\Users\kgandhi\Desktop\Trend Analysis';


libname demo1 'K:\shared\Databases\NHANES\SAS data 1999-2000\Demographic data';
libname hth1 'K:\shared\Databases\NHANES\SAS data 1999-2000\Questionnaire data\Current Health Status';
libname lab1 'K:\shared\Databases\NHANES\SAS data 1999-2000\Laboratory data\Biochemistry Profile and Hormones';
libname lab2 'K:\shared\Databases\NHANES\SAS data 1999-2000\Laboratory data\Complete Blood Count';
data trend.a1; set demo1.demo;  run; 
data trend.a2; set hth1.hsq;  run; 
data trend.a3; merge lab1.Lab18 lab2.Lab25; by SEQN; run;

data year1; merge trend.a1 nhanes.a2 trend.a3; by SEQN; run;

libname demo2 'K:\shared\Databases\NHANES\SAS data 2001-2002\Demographic data';
libname hth2 'K:\shared\Databases\NHANES\SAS data 2001-2002\Questionnaire data\Current Health Status';
libname labo1 'K:\shared\Databases\NHANES\SAS data 2001-2002\Laboratory data\Standard Biochemistry Profile';
libname labo2 'K:\shared\Databases\NHANES\SAS data 2001-2002\Laboratory data\Complete Blood Count with 5-part Differential';
data trend.b1; set demo2.demo_b;  run; 
data trend.b2; set hth2.hsq_b;  run; 
data trend.b3; merge labo1.l40_b labo2.L25_b; by SEQN; run;

data year2; merge trend.b1 trend.b2 trend.b3; by SEQN; run;


libname demo3 'K:\shared\Databases\NHANES\SAS data 2003-2004\Demographic data';
libname hth3 'K:\shared\Databases\NHANES\SAS data 2003-2004\Questionnaire data\Current Health Status';
libname labor1 'K:\shared\Databases\NHANES\SAS data 2003-2004\Laboratory data\Biochemistry Profile';
libname labor2 'K:\shared\Databases\NHANES\SAS data 2003-2004\Laboratory data\Complete Blood Count with 5-part Differential in Whole Blood';
data trend.c1; set demo3.demo_c; run;
data trend.c2; set hth3.hsq_c; run;
data trend.c3; merge labor1.l40_c labor2.l25_c; by SEQN; run;

data year3; merge trend.c1 trend.c2 trend.c3; by SEQN; run;


libname demo4 'K:\shared\Databases\NHANES\SAS data 2005-2006\Demographic data';
libname hth4 'K:\shared\Databases\NHANES\SAS data 2005-2006\Questionnaire data\Current Health Status';
libname labora1 'K:\shared\Databases\NHANES\SAS data 2005-2006\Laboratory data\Standard Biochemistry Profile';
libname labora2 'K:\shared\Databases\NHANES\SAS data 2005-2006\Laboratory data\Complete Blood Count with 5-Part in Whole Blood';
data trend.d1; set demo4.demo_d; run;
data trend.d2; set hth4.hsq_d; run;
data trend.d3; merge labora1.biopro_d labora2.cbc_d; by SEQN; run;

data year4; merge trend.d1 trend.d2 trend.d3; by SEQN; run;

libname demo5 'K:\shared\Databases\NHANES\SAS data 2007-2008\Demographic data';
libname hth5 'K:\shared\Databases\NHANES\SAS data 2007-2008\Questionnaire data\Current Health Status';
libname laborat1 'K:\shared\Databases\NHANES\SAS data 2007-2008\Laboratory data\Standard Biochemistry Profile';
libname laborat2 'K:\shared\Databases\NHANES\SAS data 2007-2008\Laboratory data\Complete Blood Count with 5-Part Differential in Whole Blood';
data trend.e1; set demo5.demo_e; run;
data trend.e2; set hth5.hsq_e; run;
data trend.e3; merge laborat1.biopro_e laborat2.cbc_e; by SEQN; run;

data year5; merge trend.e1 trend.e2 trend.e3; by SEQN; run;

libname demo6 'K:\shared\Databases\NHANES\SAS data 2009-2010\Demographic data';
libname hth6 'K:\shared\Databases\NHANES\SAS data 2009-2010\Questionnaire data\Current Health Status';
libname la1 'K:\shared\Databases\NHANES\SAS data 2009-2010\Laboratory data\Standard Biochemistry Profile';
libname la2 'K:\shared\Databases\NHANES\SAS data 2009-2010\Laboratory data\Complete Blood Count with 5-Part Differential in Whole Blood';
data trend.f1; set demo6.demo_f; run;
data trend.f2; set hth6.hsq_f; run;
data trend.f3; merge la1.biopro_f la2.cbc_f; by SEQN; run;

data year6; merge trend.f1 trend.f2 trend.f3; by SEQN; run;

libname demo7 'K:\shared\Databases\NHANES\SAS data 2011-2012\Demographic data';
libname hth7 'K:\shared\Databases\NHANES\SAS data 2011-2012\Questionnaire data\Current Health Status';
libname lb1 'K:\shared\Databases\NHANES\SAS data 2011-2012\Laboratory data\Standard Biochemistry Profile';
libname lb2 'K:\shared\Databases\NHANES\SAS data 2011-2012\Laboratory data\Complete Blood Count with 5-Part Differential in Whole Blood';
data trend.g1; set demo7.demo_g; run;
data trend.g2; set hth7.hsq_g; run;
data trend.g3; merge lb1.biopro_g lb2.cbc_g; by SEQN; run;

data year7; merge trend.g1 trend.g2 trend.g3; by SEQN; run;

data trend; merge year1 year2 year3 year4 year5 year6 year7; by SEQN; run;

data trend.trend; set trend; if sddsrvyr=1 or sddsrvyr=2 then WTMEC14YR = (2/7)*WTMEC4YR ; * for 1999-2002;
else WTMEC14YR = (1/7)*WTMEC2YR ; * for 2003-2004;
run;

data new; set trend.trend; where RIDAGEYR >= 18 and RIDAGEYR <= 65; run;










