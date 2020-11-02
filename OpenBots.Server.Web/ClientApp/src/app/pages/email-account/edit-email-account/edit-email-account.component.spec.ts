import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditEmailAccountComponent } from './edit-email-account.component';

describe('EditEmailAccountComponent', () => {
  let component: EditEmailAccountComponent;
  let fixture: ComponentFixture<EditEmailAccountComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditEmailAccountComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditEmailAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
