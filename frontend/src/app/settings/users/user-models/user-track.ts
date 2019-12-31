export class Recommendations {
    public userId: string;
    public tracks: Array<UserUpdateTrack>;
    public modules: Array<UserUpdateModule>;
    public events: Array<UserUpdateEvent>;
}

export class UserUpdateTrack {
    public id: string;
    public name: string;
    public level: number;
    public percentage: number;
}

export class UserUpdateModule {
    public id: string;
    public name: string;
}

export class UserUpdateEvent {
    public scheduleId: string;
    public eventId: string;
    public name: string;
}
