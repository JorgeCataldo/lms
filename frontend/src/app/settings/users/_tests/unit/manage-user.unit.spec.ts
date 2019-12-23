import { SettingsManageUserComponent } from '../../manage-user/manage-user.component';
import { userInfoMock, loggedUserMock } from '../mocks';
import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { SharedService } from 'src/app/shared/services/shared.service';
import { ExternalService } from 'src/app/shared/services/external.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { of } from 'rxjs';
import { CategoryEnum } from 'src/app/models/enums/category.enum';
import { Router, ActivatedRoute } from '@angular/router';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { UploadService } from 'src/app/shared/services/upload.service';

const userInfo = userInfoMock;
const loggedData = loggedUserMock;

describe('[Unit] SettingsManageUserComponent', () => {

  let fixture: ComponentFixture<SettingsManageUserComponent>;
  let component: SettingsManageUserComponent;

  let userServices: SettingsUsersService;
  let authServices: AuthService;
  let routerServices: Router;
  let activeRouteServices: ActivatedRoute;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        ListSearchModule
      ],
      declarations: [
        SettingsManageUserComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        SettingsUsersService,
        SharedService,
        UploadService,
        ExternalService,
        AuthService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SettingsManageUserComponent);
      component = fixture.componentInstance;
      userServices = TestBed.get(SettingsUsersService);
      authServices = TestBed.get(AuthService);
      routerServices = TestBed.get(Router);
      activeRouteServices = TestBed.get(ActivatedRoute);
    });
  }));

  it('Should load categories.', () => {
    const serv = spyOn(
      userServices, 'getUserCategory'
    ).and.callFake(() => of({ data: { items: [] } }));

    component['_loadCategories']();
    fixture.detectChanges();

    expect(serv).toHaveBeenCalledWith(CategoryEnum.BusinessGroups);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.BusinessUnits);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.FrontBackOffices);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Jobs);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Ranks);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Sectors);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Segments);
    // expect(serv).toHaveBeenCalledWith(CategoryEnum.Users);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Countries);
    expect(serv).toHaveBeenCalledWith(CategoryEnum.Locations);
  });

  it('Should save user info.', () => {
    const updatedUser = spyOn(
      userServices, 'createUpdateUser'
    ).and.callFake(() => of(null));

    spyOn(
      activeRouteServices.snapshot.paramMap, 'get'
    ).and.callFake(() => userInfo.id );

    loggedData.role = 'testRole';
    loggedData.user_id = userInfo.id;

    const loggedUser = spyOn(
      authServices, 'getLoggedUser'
    ).and.callFake(() => loggedData);

    const logout = spyOn(
      authServices, 'logout'
    );

    spyOn(
      userServices, 'getUserCategory'
    ).and.callFake(() => of({ data: [] }));
    fixture.detectChanges();

    component.segments = [];
    component.formGroup = component['_createUserForm'](null);
    component.userRole = userInfo.role;

    fixture.detectChanges();

    component.save();

    expect(updatedUser).toHaveBeenCalled();
    expect(loggedUser).toHaveBeenCalled();
    expect(logout).toHaveBeenCalled();
  });

  it('Should load user', () => {
    spyOn(
      userServices, 'getUserCategory'
    ).and.callFake(() => of({ data: [] }));

    const userById = spyOn(
      userServices, 'getUserById'
    ).and.callFake(() => of({ data: [] }));

    spyOn(
      authServices, 'getLoggedUser'
    ).and.callFake(() => loggedData);

    fixture.detectChanges();

    component['_loadUser'](userInfo.id);

    expect(userById).toHaveBeenCalled();
    expect(component.formGroup).toBeDefined();
    expect(component.formGroup).not.toBeNull();

  });

  it('Should set user info', () => {
    spyOn(
      activeRouteServices.snapshot.paramMap, 'get'
    ).and.callFake(() => userInfo.id );

    spyOn(
      userServices, 'getUserCategory'
    ).and.callFake(() => of({ data: [] }));

    component.userRole = userInfo.role;

    component.formGroup = component['_createUserForm']();

    expect(component['_activatedRoute'].snapshot.paramMap.get('userId')).toBeDefined();
    expect(component.userRole).toBeDefined();

    const user = component['_setUserInfo'](component.formGroup.getRawValue());
    const setUser = component['_setUserInfo'](user);

    expect(setUser).toBeDefined();
    expect(setUser.id).not.toBeNull();
    expect(setUser.lineManager).not.toBeNaN();
    expect(setUser.businessGroup).toBeDefined();
    expect(setUser.businessUnit).toBeDefined();
    expect(setUser.frontBackOffice).toBeDefined();
    expect(setUser.job).toBeDefined();
    expect(setUser.rank).toBeDefined();
    expect(setUser.address).toBeDefined();
    expect(setUser.address.city).toBeDefined();
    expect(setUser.address.zipCode).toBeDefined();
    expect(setUser.address.state).toBeDefined();
    expect(setUser.address.district).toBeDefined();
    expect(setUser.emitDate).toBeDefined();
    expect(setUser.expirationDate).toBeDefined();
    expect(setUser.sectors).toBeDefined();
  });

  it('should navigate to change of password', () => {
    const rout = spyOn(routerServices, 'navigate');
    component.changePassword();
    const id = component['_activatedRoute'].snapshot.paramMap.get('userId');
    expect(id).toBeDefined();
    expect(rout).toHaveBeenCalled();
  });

});
