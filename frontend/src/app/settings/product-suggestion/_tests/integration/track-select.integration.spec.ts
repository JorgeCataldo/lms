import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { tracksMock } from '../mocks';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material';
import { SuggestionTrackSelectComponent } from '../../manage-suggestion/track-select/track-select.component';

const dbTrack = tracksMock[0];

describe('[Integration] SuggestionTrackSelectComponent', () => {

  let fixture: ComponentFixture<SuggestionTrackSelectComponent>;
  let component: SuggestionTrackSelectComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        FormsModule,
        MatCheckboxModule
      ],
      declarations: [
        SuggestionTrackSelectComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(SuggestionTrackSelectComponent);
      component = fixture.componentInstance;
      component.track = dbTrack;
    });
  }));

  it('should display the track image', async () => {
    fixture.detectChanges();
    const imgElement = fixture.nativeElement.querySelector('img.main-img');
    expect(imgElement.src).toContain(dbTrack.imageUrl);
  });

  it('should display the track title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview h3');
    expect(titleElement.textContent).toContain(dbTrack.title);
  });

  it('should display the tracks modules and events count', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('div.preview p');
    expect(titleElement.textContent).toContain('10 MÓDULOS');
    expect(titleElement.textContent).toContain('3 EVENTOS');
  });
});
