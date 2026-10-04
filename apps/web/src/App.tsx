import {
  createBrowserRouter,
  RouterProvider,
} from 'react-router-dom'
import AppLayout from './components/AppLayout'
import DashboardPage from './pages/DashboardPage'
import PlaceholderPage from './pages/PlaceholderPage'
import StudentDetailPage from './pages/StudentDetailPage'
import StudentsPage from './pages/StudentsPage'
import './App.css'
import NewStudentPage from './pages/NewStudentPage'
import EditStudentPage from './pages/EditStudentPage'
import BeltExamsPage from './pages/BeltExamsPage'
import BeltExamDetailPage from './pages/BeltExamDetailPage'
import BeltExamStudentResultPage from './pages/BeltExamStudentResultPage'

const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
      {
        path: 'students',
        element: <StudentsPage />,
      },
      {
  path: 'students/new',
  element: <NewStudentPage />,
},
{
  path: 'students/:studentId/edit',
  element: <EditStudentPage />,
},
{
  path: 'students/:studentId',
  element: <StudentDetailPage />,
},
      {
        path: 'belt-exams',
        element: <BeltExamsPage />,
      },
      {
        path: 'belt-exams/:beltExamId',
        element: <BeltExamDetailPage />,
      },
      {
        path: 'belt-exams/:beltExamId/results/:studentResultId',
        element: <BeltExamStudentResultPage />,
      },
      {
        path: 'attendance',
        element: (
          <PlaceholderPage
            eyebrow="ĐIỂM DANH"
            title="Điểm danh"
            description="Quản lý điểm danh môn sinh theo buổi tập."
          />
        ),
      },
      {
        path: 'tuition',
        element: (
          <PlaceholderPage
            eyebrow="HỌC PHÍ"
            title="Học phí"
            description="Theo dõi học phí, miễn giảm và khoản dư của môn sinh."
          />
        ),
      },
      {
        path: 'receipts',
        element: (
          <PlaceholderPage
            eyebrow="PHIẾU THU"
            title="Phiếu thu"
            description="Thu tiền và theo dõi lịch sử thanh toán."
          />
        ),
      },
      {
        path: 'classes',
        element: (
          <PlaceholderPage
            eyebrow="LỚP HỌC"
            title="Lớp học"
            description="Quản lý lớp, lịch tập và huấn luyện viên."
          />
        ),
      },
    ],
  },
])

function App() {
  return <RouterProvider router={router} />
}

export default App
