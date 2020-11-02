import {Component, Input, forwardRef} from '@angular/core';
import {NG_VALUE_ACCESSOR} from '@angular/forms';

@Component({
  selector: 'ngx-validation-message',
  styleUrls: ['./validation-message.component.scss'],
  template: `
      <div class="warning">
          <span class="caption status-danger"
             *ngIf="showMinLength"> Minimum length is {{ minLength }} characters  </span>
          <span class="caption status-danger"
             *ngIf="showMaxLength"> Maximum length is {{ maxLength }} characters  </span>
          <span class="caption status-danger" *ngIf="showPattern"> Incorrect {{ label }} </span>
          <span class="caption status-danger" *ngIf="showRequired"> {{ label }} is required</span>
          <span class="caption status-danger" *ngIf="showMin">Min value of {{ label }} is {{ min }}</span>
          <span class="caption status-danger" *ngIf="showMax">Max value of {{ label }} is {{ max }}</span>
      </div>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NgxValidationMessageComponent),
      multi: true,
    },
  ],
})
export class NgxValidationMessageComponent {
  @Input()
  label: string = '';

  @Input()
  showRequired?: boolean;

  @Input()
  min?: number;

  @Input()
  showMin?: boolean;

  @Input()
  max?: number;

  @Input()
  showMax: boolean;

  @Input()
  minLength?: number;

  @Input()
  showMinLength?: boolean;

  @Input()
  maxLength?: number;

  @Input()
  showMaxLength?: boolean;

  @Input()
  showPattern?: boolean;
}
