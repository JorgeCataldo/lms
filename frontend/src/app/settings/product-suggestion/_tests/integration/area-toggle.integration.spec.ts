import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { SuggestionAreaToggleComponent } from '../../manage-suggestion/area-toggle/area-toggle.component';

describe('[Integration] SettingsTestCardComponent', () => {

  let fixture: ComponentFixture<SuggestionAreaToggleComponent>;
  let component: SuggestionAreaToggleComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule
      ],
      declarations: [
        SuggestionAreaToggleComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SuggestionAreaToggleComponent);
      component = fixture.componentInstance;
      (component as any).title = 'Area Title';
    });
  }));

  it('should display the title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('h2.toggle');
    expect(titleElement.textContent).toContain('Area Title');
  });

  it('should emit Edit Test event', async () => {
    const toggleSpy = spyOn(component.toggle, 'emit');

    const toggleImage = fixture.nativeElement.querySelector('span img');
    toggleImage.dispatchEvent(new Event('click'));
    fixture.detectChanges();

    expect(toggleSpy).toHaveBeenCalled();
  });

});
