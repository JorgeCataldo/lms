import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { ActivatedRoute } from '@angular/router';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { of } from 'rxjs';
import { SettingsUserRecommendationComponent } from '../../user-recommendation/user-recommendation.component';
import {
  SettingsUserCareerRecommendationComponent
} from '../../user-recommendation/user-career-recommendation/user-career-recommendation.component';
import {
  SettingsUserDetailsSummaryRecommendationComponent
} from '../../user-recommendation/user-summary-recommendation/user-summary-recommendation.component';
import { SettingsProseekRecommendationComponent } from '../../user-recommendation/proseek-recommendation/proseek-recommendation.component';
import {
  RecommendationRadarComponent
} from '../../user-recommendation/proseek-recommendation/recommendation-radar/recommendation-radar.component';
import { SharedService } from 'src/app/shared/services/shared.service';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { ConceptTagModule } from 'src/app/shared/components/layout/concept-tag/concept-tag.module';
import { CardsSliderModule } from 'src/app/shared/components/cards-slider/cards-slider.module';
import { TrackCalendarModule } from 'src/app/pages/track/track-overview/track-calendar/track-calendar.module';
import { userRecommendationMock } from '../mocks';
// tslint:disable-next-line: max-line-length
import { RecommendationBarComponent } from '../../user-recommendation/proseek-recommendation/recommendation-bar/recommendation-bar.component';
import { ProfileRadarComponent } from '../../user-recommendation/proseek-recommendation/profile-radar/profile-radar.component';

describe('[Unit] SettingsUserRecommendationComponent', () => {

  let fixture: ComponentFixture<SettingsUserRecommendationComponent>;
  let component: SettingsUserRecommendationComponent;

  let service: SettingsUsersService;
  let sharedService: SharedService;
  let activatedRoute: ActivatedRoute;
  let recruitmentService: RecruitingCompanyService;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        ProgressBarModule,
        ConceptTagModule,
        CardsSliderModule,
        TrackCalendarModule
      ],
      declarations: [
        SettingsUserRecommendationComponent,
        SettingsUserCareerRecommendationComponent,
        SettingsUserDetailsSummaryRecommendationComponent,
        SettingsProseekRecommendationComponent,
        RecommendationRadarComponent,
        RecommendationBarComponent,
        ProfileRadarComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        SettingsUsersService,
        SharedService,
        RecruitingCompanyService,
        AuthService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SettingsUserRecommendationComponent);
      component = fixture.componentInstance;
      service = TestBed.get(SettingsUsersService);
      sharedService = TestBed.get(SharedService);
      recruitmentService = TestBed.get(RecruitingCompanyService);
      activatedRoute = TestBed.get(ActivatedRoute);
    });
  }));

  it('should call the endpoint to retrieve the user recommendation info from the server', () => {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '123456');

    const userInfoSpy = spyOn(
      service, 'getUserRecommendation'
    ).and.callFake(() => of({ data: userRecommendationMock }));

    const userSkillsSpy = spyOn(
      service, 'getUserSkills'
    ).and.callFake(() => of({ data: { }}));

    const levelsSpy = spyOn(
      sharedService, 'getLevels'
    ).and.callFake(() => of({ data: [] }));

    component.ngOnInit();
    fixture.detectChanges();

    expect(userInfoSpy).toHaveBeenCalledWith('123456');
    expect(userSkillsSpy).toHaveBeenCalledWith('123456');
    expect(levelsSpy).toHaveBeenCalledWith(true);
  });

  it('should not call the endpoint to toggle the permission of the user recommendation card visualization', () => {
    const serviceSpy = spyOn(
      service, 'allowRecommendation'
    ).and.callFake(() => of({ data: { } }));

    setOnInitSpies();
    component.userId = '123456';
    component.allowRecommendation(true);
    component.allowRecommendation(false);
    expect(serviceSpy).not.toHaveBeenCalled();
  });

  it('should call the endpoint to toggle the permission of the user recommendation card visualization', () => {
    const serviceSpy = spyOn(
      service, 'allowRecommendation'
    ).and.callFake(() => of({ data: { } }));

    setOnInitSpies();
    component.userId = '123456';
    component.isCurrentUserCard = true;

    component.allowRecommendation(true);
    fixture.detectChanges();
    expect(serviceSpy).toHaveBeenCalledWith('123456', true);
  });

  it('should not call the endpoint to toggle the permission of the secretary recommendation card visualization', () => {
    const serviceSpy = spyOn(
      service, 'allowSecretaryRecommendation'
    ).and.callFake(() => of({ data: { } }));

    setOnInitSpies();
    component.userId = '123456';
    component.currentUserRole = 'Student';

    component.allowSecretaryRecommendation(true);
    component.allowSecretaryRecommendation(false);
    fixture.detectChanges();
    expect(serviceSpy).not.toHaveBeenCalled();
  });

  it('should call the endpoint to toggle the permission of the secretary recommendation card visualization', () => {
    const serviceSpy = spyOn(
      service, 'allowSecretaryRecommendation'
    ).and.callFake(() => of({ data: { } }));

    setOnInitSpies();
    component.userId = '123456';
    component.currentUserRole = 'Secretary';

    component.allowSecretaryRecommendation(true);
    fixture.detectChanges();
    expect(serviceSpy).toHaveBeenCalledWith('123456', true);
  });

  it('should call the endpoint to toggle the user as a favorite of a recruiter', () => {
    const addSpy = spyOn(
      recruitmentService, 'addRecruitmentFavorite'
    ).and.callFake(() => of({ data: { }}));

    const removeSpy = spyOn(
      recruitmentService, 'removeRecruitmentFavorite'
    ).and.callFake(() => of({ data: { }}));

    setOnInitSpies();
    component.ngOnInit();
    fixture.detectChanges();

    component.user.isFavorite = true;
    component.toggleUserToFavorites();
    fixture.detectChanges();
    expect(removeSpy).toHaveBeenCalledWith('123456');

    component.user.isFavorite = false;
    component.toggleUserToFavorites();
    fixture.detectChanges();
    expect(addSpy).toHaveBeenCalledWith('123456');
  });

  function setOnInitSpies() {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '123456');
    spyOn(service, 'getUserRecommendation').and.callFake(() => of({ data: userRecommendationMock }));
    spyOn(service, 'getUserSkills').and.callFake(() => of({ data: { }}));
    spyOn(sharedService, 'getLevels').and.callFake(() => of({ data: [] }));
  }

});
