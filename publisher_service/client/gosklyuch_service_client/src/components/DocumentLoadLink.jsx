import axios from "axios"
import { useEffect } from "react"

export const DocumentLoadLink = ({res}) => {
    return <a href={`${axios.defaults.baseURL}/documents/${res}`} >Скачать</a>
} 