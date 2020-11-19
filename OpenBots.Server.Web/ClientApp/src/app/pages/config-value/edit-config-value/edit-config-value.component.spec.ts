import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditConfigValueComponent } from './edit-config-value.component';

describe('EditConfigValueComponent', () => {
  let component: EditConfigValueComponent;
  let fixture: ComponentFixture<EditConfigValueComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditConfigValueComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditConfigValueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
