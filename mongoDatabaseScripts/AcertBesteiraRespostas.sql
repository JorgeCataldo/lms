
--drop table #de_para
select *, cast(null as varchar(100)) oldQuestion, cast(null as varchar(100)) oldanswer, cast(0 as bit) as excluido 
into #de_para
from bdq_new

--delete from bdq_old_29_03 where moduleId = '5ba8852703579766846d4568'
--insert into bdq_old_29_03 select * from bdq_old_12_03 where moduleId = '5ba8852703579766846d4568'

delete n
from #de_para n
join bdq_old_29_03 o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and n.questionid = o.questionid


update n
set oldQuestion = o.questionId
from #de_para n
join bdq_old_29_03 o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and n.questionname = o.questionname


update n
set OldAnswer = o.answerid
from #de_para n
join bdq_old_29_03 o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and n.oldQuestion = o.questionid
	and n.answername = o.answername

	
update n
set oldQuestion = o.questionId
from #de_para n
join bdq_old_29_03 o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and left(n.questionname,100) = left(o.questionname,100)
where oldQuestion is null

--drop table #orphan
select *, cast(null as varchar(100)) as NewQuestionId, cast(null as varchar(100)) as NewAnwserId, cast(0 as bit) original 
into #orphan
from answers a 


update o
set original = 1
from #orphan o 
join bdq_new t on o.moduleid = t.moduleid
		and o.subjectid = t.subjectid
		and (o.questionid = t.questionid)


update o
set newQuestionId = t.questionId
from #orphan o 
join #de_para t on o.moduleid = t.moduleid
		and o.subjectid = t.subjectid
		and (o.questionid = t.oldQuestion)


update o
set NewAnwserId = t.answerId
from #orphan o 
join #de_para t on o.moduleid = t.moduleid
		and o.subjectid = t.subjectid
		and (o.questionid = t.oldQuestion)
		and (o.answerId = t.oldanswer)

select (select count(1) from #orphan where original = 1) as semalteracao,
(select count(1) from #orphan where NewQuestionId is not null) as recuperada,
(select count(1) from #orphan where NewQuestionId is null and original= 0) as perda

select DATEADD(d, Cast(cast(answerDate as bigint) * POWER(10.00000000000,-7) / 60 / 60 / 24 As int), Cast('0001-01-01' As DATE)),
*
from #orphan where NewQuestionId is null and original= 0
order by 1 desc

Set @TickValue = 634024345696365272 
Select @Days = cast(answerDate as bigint) * POWER(10.00000000000,-7) / 60 / 60 / 24

Select DATEADD(d, Cast(cast(answerDate as bigint) * POWER(10.00000000000,-7) / 60 / 60 / 24 As int), Cast('0001-01-01' As DATE))


select distinct moduleId, modulename, subjectid, subjectname 
into #modules
from bdq_new 

select distinct o.moduleId, modulename, o.subjectid, subjectname from #orphan n
join #modules o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 

	
select * from #orphan n
join drafts o on n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and n.questionid = o.questionid
	
drop table  #aggregate
select moduleId, subjectid, questionid, tablename
into #aggregate
from (
select moduleId, subjectid, questionid, 'bdq_old_12_03' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_29_03' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_02' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_03' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_04' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_05' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_06' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_07' as tablename from bdq_old_12_03
union all 
select moduleId, subjectid, questionid, 'bdq_old_04_08' as tablename from bdq_old_12_03
)a


select * 
from #orphan n
where exists (
select 1 from #aggregate o where n.moduleid = o.moduleid 
	and n.subjectid = o.subjectid 
	and n.questionid = o.questionid
	)

select * from #de_para