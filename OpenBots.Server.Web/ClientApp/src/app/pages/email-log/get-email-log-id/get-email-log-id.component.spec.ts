import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetEmailLogIdComponent } from './get-email-log-id.component';

describe('GetEmailLogIdComponent', () => {
  let component: GetEmailLogIdComponent;
  let fixture: ComponentFixture<GetEmailLogIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetEmailLogIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetEmailLogIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
