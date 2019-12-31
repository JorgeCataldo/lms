export interface UserJobsPosition {
    totalOpenJobs: number;
    userApplications: UserApplications[];
}

export interface UserApplications {
    jobPositionId: string;
    jobTitle: string;
    recruitingCompanyName: string;
    dueTo: Date;
    approved?: boolean;
    createdBy: string;
    accepted: boolean;
}

export interface UserJobsNotifications {
    notificationId: string;
    title: string;
    text: string;
    createdAt: Date;
    read: boolean;
}
