import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailTestingAccountComponent } from './email-testing-account.component';

describe('EmailTestingAccountComponent', () => {
  let component: EmailTestingAccountComponent;
  let fixture: ComponentFixture<EmailTestingAccountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EmailTestingAccountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EmailTestingAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
