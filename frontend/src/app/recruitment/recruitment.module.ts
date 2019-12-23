import { NgModule } from '@angular/core';
import { RecruitingCompanyModule } from './recruiting-company/recruiting-company.module';
import { ManageTalentsModule } from './manage-talents/manage-talents.module';
import { ManageJobApplicationsModule } from './job-applications/job-applications.module';
import { ManageJobsModule } from './manage-jobs/manage-jobs.module';
import { RecJobsModule } from './student-job/rec-jobs/rec-jobs.module';
import { StudentJobsModule } from './student-job/jobs/student-jobs.module';
import { StudentSearchJobModule } from './student-job/student-search-job/student-search-job.module';
import { CandidacyStudentsModule } from './student-job/candidacy/candidacy-students.module';


@NgModule({
  imports: [
    RecruitingCompanyModule,
    ManageTalentsModule,
    ManageJobsModule,
    ManageJobApplicationsModule,
    RecJobsModule,
    StudentJobsModule,
    StudentSearchJobModule,
    CandidacyStudentsModule
  ]
})
export class RecruitmentModule { }
