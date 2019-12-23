import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-valuation-test-release-toggle',
  templateUrl: './valuation-test-release-toggle.html',
  styleUrls: ['./valuation-test-release-toggle.component.scss']
})
export class ValuationTestReleaseToggleComponent {

  @Input() readonly title: string;
  @Input() readonly isClosed: boolean;
  @Output() toggle: EventEmitter<boolean> = new EventEmitter();

}
