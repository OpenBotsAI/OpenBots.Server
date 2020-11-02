import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetEmailIdComponent } from './get-email-id.component';

describe('GetEmailIdComponent', () => {
  let component: GetEmailIdComponent;
  let fixture: ComponentFixture<GetEmailIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetEmailIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetEmailIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
