import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { GetFileIdComponent } from './get-file-id.component';

describe('GetFileIdComponent', () => {
  let component: GetFileIdComponent;
  let fixture: ComponentFixture<GetFileIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [GetFileIdComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetFileIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
