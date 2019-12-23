import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { SettingsTestResultsCardComponent } from '../../test-results-card/test-results-card.component';
import { BrowserModule } from '@angular/platform-browser';
import { profileTestsMock } from '../mocks';

describe('[Integration] SettingsTestCardComponent', () => {

  /*let fixture: ComponentFixture<SettingsTestCardComponent>;
  let component: SettingsTestCardComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule
      ],
      declarations: [
        SettingsTestCardComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SettingsTestCardComponent);
      component = fixture.componentInstance;
      component.test = profileTestsMock[0];
    });
  }));

  it('should display the test Title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('h3');
    expect(titleElement.textContent).toContain(profileTestsMock[0].title);
  });

  it('should emit Edit Test event', async () => {
    const editTestSpy = spyOn(component.editTest, 'emit');

    const editTestImage = fixture.nativeElement.querySelector('div.edit img:first-child');
    editTestImage.dispatchEvent(new Event('click'));

    fixture.detectChanges();
    expect(editTestSpy).toHaveBeenCalled();
  });

  it('should emit Delete Test event', async () => {
    const deleteTestSpy = spyOn(component.deleteTest, 'emit');

    const deleteTestImage = fixture.nativeElement.querySelector('div.edit img:last-child');
    deleteTestImage.dispatchEvent(new Event('click'));

    fixture.detectChanges();
    expect(deleteTestSpy).toHaveBeenCalled();
  });

  it('should emit Get Answers Excel event', async () => {
    const getExcelSpy = spyOn(component.getAnswersExcel, 'emit');

    const exportExcelButton = fixture.nativeElement.querySelector('div.preview p.content span');
    exportExcelButton.dispatchEvent(new Event('click'));

    fixture.detectChanges();
    expect(getExcelSpy).toHaveBeenCalled();
  });*/

});
